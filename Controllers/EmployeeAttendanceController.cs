using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize]
    public class EmployeeAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EmployeeAttendanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Employee Login (redirect to main login)
        public IActionResult Login()
        {
            return RedirectToAction("Login", "Account");
        }

        // GET: Employee Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var today = DateTime.Today;
            var todayAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date == today);

            ViewBag.Employee = employee;
            ViewBag.TodayAttendance = todayAttendance;
            ViewBag.CurrentTime = DateTime.Now;

            return View();
        }

        // GET: Leave Request Form
        public async Task<IActionResult> LeaveRequest()
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Employee = employee;
            return View();
        }

        // POST: Submit Leave Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveRequest(Leave leave)
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                TempData["ErrorMessage"] = "Please login first";
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee not found";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Populate server-controlled fields and re-validate the model
                // EmployeeId and ApprovalStatus are [Required] but not posted from the form
                leave.EmployeeId = employee.Id;
                leave.ApprovalStatus = "Pending";

                // Clear existing model state (which may have errors for the above fields)
                ModelState.Clear();

                // Re-validate with server-populated values
                if (TryValidateModel(leave))
                {
                    // Validate dates
                    if (leave.StartDate < DateTime.Today)
                    {
                        ModelState.AddModelError("StartDate", "Start date cannot be in the past");
                        ViewBag.Employee = employee;
                        return View(leave);
                    }

                    if (leave.EndDate < leave.StartDate)
                    {
                        ModelState.AddModelError("EndDate", "End date must be after start date");
                        ViewBag.Employee = employee;
                        return View(leave);
                    }

                    leave.CreatedAt = DateTime.UtcNow;
                    leave.UpdatedAt = DateTime.UtcNow;

                    _context.Leaves.Add(leave);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Leave request submitted successfully! Your request is pending approval.";
                    return RedirectToAction("MyLeaves");
                }
                else
                {
                    TempData["ErrorMessage"] = "Please correct the errors below";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your leave request. Please try again.";
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Leave request error: {ex.Message}");
            }

            ViewBag.Employee = employee;
            return View(leave);
        }

        // GET: My Leave Requests
        public async Task<IActionResult> MyLeaves()
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var leaves = await _context.Leaves
                .Where(l => l.EmployeeId == employee.Id)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(leaves);
        }

        // GET: My Attendance History
        public async Task<IActionResult> MyAttendance(DateTime? from, DateTime? to)
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var startDate = from ?? DateTime.Today.AddMonths(-1);
            var endDate = to ?? DateTime.Today;

            var attendances = await _context.Attendances
                .Where(a => a.EmployeeId == employee.Id && a.Date >= startDate && a.Date <= endDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            ViewBag.From = startDate;
            ViewBag.To = endDate;
            ViewBag.Employee = employee;

            return View(attendances);
        }

        // POST: Check In
        [HttpPost]
        public async Task<IActionResult> CheckIn(string location = "")
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                return Json(new { success = false, message = "Please login first" });
            }

            var employee = await _context.Employees
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            try
            {
                var today = DateTime.Today;
                var now = DateTime.Now;

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date == today);

                if (attendance == null)
                {
                    attendance = new Attendance
                    {
                        EmployeeId = employee.Id,
                        Date = today,
                        CheckInTime = now,
                        Location = location,
                        Status = DetermineAttendanceStatus(now, employee.Shift),
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.Attendances.Add(attendance);
                }
                else if (attendance.CheckInTime == null)
                {
                    attendance.CheckInTime = now;
                    attendance.Location = location;
                    attendance.Status = DetermineAttendanceStatus(now, employee.Shift);
                    attendance.UpdatedAt = now;
                }
                else
                {
                    return Json(new { success = false, message = "Already checked in today" });
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Check-in successful at " + now.ToString("HH:mm:ss"), checkInTime = now.ToString("HH:mm:ss") });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Check-in error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred during check-in. Please try again." });
            }
        }

        // POST: Check Out
        [HttpPost]
        public async Task<IActionResult> CheckOut()
        {
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                return Json(new { success = false, message = "Please login first" });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            try
            {
                var today = DateTime.Today;
                var now = DateTime.Now;

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date == today);

                if (attendance == null || attendance.CheckInTime == null)
                {
                    return Json(new { success = false, message = "Please check in first" });
                }

                if (attendance.CheckOutTime != null)
                {
                    return Json(new { success = false, message = "Already checked out today" });
                }

                attendance.CheckOutTime = now;
                attendance.TotalHours = CalculateTotalHours(attendance.CheckInTime.Value, now);
                attendance.UpdatedAt = now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Check-out successful at " + now.ToString("HH:mm:ss"), checkOutTime = now.ToString("HH:mm:ss") });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Check-out error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred during check-out. Please try again." });
            }
        }

        // GET: Change Password
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Change Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Password changed successfully";
                    return RedirectToAction("Dashboard");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // Logout (redirect to main logout)
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        private string DetermineAttendanceStatus(DateTime checkInTime, Shift? shift)
        {
            if (shift == null) return "Present";

            var checkInTimeOnly = checkInTime.TimeOfDay;
            var lateThreshold = shift.StartTime.Add(TimeSpan.FromMinutes(15)); // 15 minutes grace period

            return checkInTimeOnly <= lateThreshold ? "On-time" : "Late";
        }

        private decimal CalculateTotalHours(DateTime checkIn, DateTime checkOut)
        {
            var duration = checkOut - checkIn;
            return Math.Round((decimal)duration.TotalHours, 2);
        }
    }
}
