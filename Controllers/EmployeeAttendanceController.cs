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
using System.Net.Http;
using System.Text.Json;


namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize]
    public class EmployeeAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmployeeAttendanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _httpClientFactory = httpClientFactory;
        }

        // GET: Employee Login (redirect to main login)
        public IActionResult Login()
        {
            return RedirectToAction("Login", "Account");
        }

        // GET: Employee Dashboard - Redirect to Member Dashboard
        public IActionResult Dashboard()
        {
            // Redirect to the Member Dashboard where attendance system is integrated
            return RedirectToAction("Member", "Dashboard");
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
            System.Diagnostics.Debug.WriteLine($"Leave request submitted: LeaveType={leave.LeaveType}, StartDate={leave.StartDate}, EndDate={leave.EndDate}, NumberOfDays={leave.NumberOfDays}");
            
            var employeeId = User.Identity?.Name;
            if (string.IsNullOrEmpty(employeeId))
            {
                System.Diagnostics.Debug.WriteLine("Leave request failed: No employee ID");
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
                System.Diagnostics.Debug.WriteLine($"Employee found: {employee.EmployeeId}, ID: {employee.Id}");
                
                // Populate server-controlled fields and re-validate the model
                // EmployeeId and ApprovalStatus are [Required] but not posted from the form
                leave.EmployeeId = employee.Id;
                leave.ApprovalStatus = "Pending";

                System.Diagnostics.Debug.WriteLine($"Model populated: EmployeeId={leave.EmployeeId}, ApprovalStatus={leave.ApprovalStatus}");

                // Clear existing model state (which may have errors for the above fields)
                ModelState.Clear();

                // Re-validate with server-populated values
                if (TryValidateModel(leave))
                {
                    System.Diagnostics.Debug.WriteLine("Model validation passed");
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
                    System.Diagnostics.Debug.WriteLine("Model validation failed");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation error: {error.ErrorMessage}");
                    }
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
        [ValidateAntiForgeryToken]
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
                    // Try to reverse geocode to a readable address
                    if (!string.IsNullOrEmpty(location) && TryParseCoordinates(location, out var lat, out var lng))
                    {
                        var address = await ReverseGeocodeAsync(lat, lng);
                        if (!string.IsNullOrEmpty(address))
                        {
                            var shortAddr = ShortenAddress(address);
                            attendance.Location = $"{shortAddr} ({ToInv(lat)},{ToInv(lng)} ~50m)";
                        }
                    }
                    _context.Attendances.Add(attendance);
                }
                else if (attendance.CheckInTime == null)
                {
                    attendance.CheckInTime = now;
                    attendance.Location = location;
                    attendance.Status = DetermineAttendanceStatus(now, employee.Shift);
                    attendance.UpdatedAt = now;
                    if (!string.IsNullOrEmpty(location) && TryParseCoordinates(location, out var lat2, out var lng2))
                    {
                        var address2 = await ReverseGeocodeAsync(lat2, lng2);
                        if (!string.IsNullOrEmpty(address2))
                        {
                            var shortAddr2 = ShortenAddress(address2);
                            attendance.Location = $"{shortAddr2} ({ToInv(lat2)},{ToInv(lng2)} ~50m)";
                        }
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Already checked in today" });
                }

                System.Diagnostics.Debug.WriteLine($"Check-in: Employee {employee.EmployeeId}, CheckIn: {attendance.CheckInTime}, Date: {attendance.Date}");

                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"Check-in saved successfully for employee {employee.EmployeeId}");

                return Json(new { success = true, message = "Check-in successful at " + now.ToString("HH:mm:ss"), checkInTime = now.ToString("HH:mm:ss") });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Check-in error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred during check-in. Please try again." });
            }
        }

        // GET: Get Current Attendance Status
        [HttpGet]
        public async Task<IActionResult> GetCurrentAttendanceStatus()
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
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date == today);

                if (attendance == null)
                {
                    return Json(new { 
                        success = true, 
                        hasCheckIn = false, 
                        hasCheckOut = false 
                    });
                }

                return Json(new { 
                    success = true, 
                    hasCheckIn = attendance.CheckInTime != null,
                    hasCheckOut = attendance.CheckOutTime != null,
                    checkInTime = attendance.CheckInTime?.ToString("HH:mm:ss"),
                    checkOutTime = attendance.CheckOutTime?.ToString("HH:mm:ss"),
                    totalHours = attendance.TotalHours
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get attendance status error: {ex.Message}");
                return Json(new { success = false, message = "Error retrieving attendance status" });
            }
        }

        // POST: Check Out
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(string location = "")
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
                
                System.Diagnostics.Debug.WriteLine($"Check-out attempt: Employee {employee.EmployeeId}, Today: {today}, Now: {now}");

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date == today);

                System.Diagnostics.Debug.WriteLine($"Found attendance record: {attendance != null}, CheckInTime: {attendance?.CheckInTime}, CheckOutTime: {attendance?.CheckOutTime}");
                
                if (attendance != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Attendance ID: {attendance.AttendanceId}, Employee ID: {attendance.EmployeeId}, Date: {attendance.Date}");
                }

                if (attendance == null || attendance.CheckInTime == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Check-out failed: No attendance record or no check-in time");
                    return Json(new { success = false, message = "Please check in first" });
                }

                if (attendance.CheckOutTime != null)
                {
                    return Json(new { success = false, message = "Already checked out today" });
                }

                attendance.CheckOutTime = now;
                attendance.TotalHours = CalculateTotalHours(attendance.CheckInTime.Value, now);
                if (!string.IsNullOrEmpty(location))
                {
                    // Save last known location on checkout if provided
                    attendance.Location = location;
                    if (TryParseCoordinates(location, out var lat3, out var lng3))
                    {
                        var address3 = await ReverseGeocodeAsync(lat3, lng3);
                        if (!string.IsNullOrEmpty(address3))
                        {
                            var shortAddr3 = ShortenAddress(address3);
                            attendance.Location = $"{shortAddr3} ({ToInv(lat3)},{ToInv(lng3)} ~50m)";
                        }
                    }
                }
                attendance.UpdatedAt = now;

                System.Diagnostics.Debug.WriteLine($"Check-out: Employee {employee.EmployeeId}, CheckIn: {attendance.CheckInTime}, CheckOut: {attendance.CheckOutTime}, TotalHours: {attendance.TotalHours}");
                
                // Ensure Entity Framework is tracking the changes
                _context.Attendances.Update(attendance);
                System.Diagnostics.Debug.WriteLine($"Entity Framework tracking: {_context.Entry(attendance).State}");

                try
                {
                    var changes = await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Check-out saved successfully for employee {employee.EmployeeId}. Changes: {changes}");
                    
                    // Verify the data was actually saved
                    var savedAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date == today);
                    
                    if (savedAttendance?.CheckOutTime == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"ERROR: Check-out time was not saved to database!");
                        return Json(new { success = false, message = "Failed to save check-out time. Please try again." });
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Verified: Check-out time saved as {savedAttendance.CheckOutTime}, TotalHours: {savedAttendance.TotalHours}");
                }
                catch (Exception saveEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Database save error: {saveEx.Message}");
                    return Json(new { success = false, message = "Failed to save check-out data. Please try again." });
                }

                return Json(new { success = true, message = "Check-out successful at " + now.ToString("HH:mm:ss"), checkOutTime = now.ToString("HH:mm:ss"), totalHours = attendance.TotalHours });
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

                // 1) Validate current password against Identity
                var identityPasswordOk = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

                // 2) Validate against Employee hash if Identity check fails
                var employeeIdCode = user.UserName; // stored as EmployeeId for employee accounts
                var employee = !string.IsNullOrEmpty(employeeIdCode)
                    ? await _context.Employees.AsTracking().FirstOrDefaultAsync(e => e.EmployeeId == employeeIdCode)
                    : null;

                var employeePasswordOk = false;
                if (!identityPasswordOk && employee != null)
                {
                    employeePasswordOk = VerifyPassword(model.CurrentPassword, employee.PasswordHash, employee.PasswordSalt);
                }

                if (!identityPasswordOk && !employeePasswordOk)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    TempData["ErrorMessage"] = "Current password is incorrect.";
                    return RedirectToAction("MyLeaves");
                }

                // Reset Identity password with token (works even if original unknown or mismatched)
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
                if (resetResult.Succeeded)
                {
                    // Sync Employee hash/salt store
                    if (employee != null)
                    {
                        var (hash, salt) = HashPassword(model.NewPassword);
                        employee.PasswordHash = hash;
                        employee.PasswordSalt = salt;
                        employee.UpdatedAt = DateTime.UtcNow;
                        // Ensure update persists even with default NoTracking behavior
                        _context.Employees.Update(employee);
                        await _context.SaveChangesAsync();
                    }

                    await _signInManager.RefreshSignInAsync(user);
                    TempData["SuccessMessage"] = "Password changed successfully";
                    TempData["PasswordChangeStatus"] = $"identityPasswordOk={identityPasswordOk}, employeePasswordOk={employeePasswordOk}, identityReset=true";
                    return RedirectToAction("MyLeaves");
                }
                else
                {
                    var errorList = new List<string>();
                    foreach (var err in resetResult.Errors)
                    {
                        ModelState.AddModelError("", err.Description);
                        errorList.Add(err.Description);
                    }
                    TempData["ErrorMessage"] = "Failed to change password.";
                    TempData["PasswordChangeStatus"] = $"identityPasswordOk={identityPasswordOk}, employeePasswordOk={employeePasswordOk}, errors={string.Join("|", errorList)}";
                    return RedirectToAction("MyLeaves");
                }
            }

            return View(model);
        }

        // Simple SHA256-based hash with salt (matches EmployeeController logic)
        private (string hash, string salt) HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var salt = Guid.NewGuid().ToString();
                var combined = password + salt;
                var bytes = Encoding.UTF8.GetBytes(combined);
                var hash = sha256.ComputeHash(bytes);
                return (Convert.ToBase64String(hash), salt);
            }
        }

        // Verify Employee table password
        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(storedSalt))
                return false;
            using (var sha256 = SHA256.Create())
            {
                var combined = password + storedSalt;
                var bytes = Encoding.UTF8.GetBytes(combined);
                var hash = sha256.ComputeHash(bytes);
                var computedHash = Convert.ToBase64String(hash);
                return computedHash == storedHash;
            }
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
            var lateThreshold = shift.StartTime.Add(TimeSpan.FromHours(1));

            return checkInTimeOnly <= lateThreshold ? "On-time" : "Late";
        }

        private decimal CalculateTotalHours(DateTime checkIn, DateTime checkOut)
        {
            var duration = checkOut - checkIn;
            return Math.Round((decimal)duration.TotalHours, 2);
        }

        private string ToInv(double val) => val.ToString(System.Globalization.CultureInfo.InvariantCulture);

        private string ShortenAddress(string full)
        {
            if (string.IsNullOrWhiteSpace(full)) return full;
            // Keep first 3 significant parts for a short, readable address
            var parts = full.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();
            if (parts.Count <= 3) return full;
            return string.Join(", ", parts.Take(3));
        }

        // Extract lat/lng from a string formatted like "lat,lng (~accm)" or "lat,lng"
        private bool TryParseCoordinates(string location, out double lat, out double lng)
        {
            lat = 0; lng = 0;
            try
            {
                if (string.IsNullOrWhiteSpace(location)) return false;
                var firstPart = location.Split(' ')[0]; // take before space
                var parts = firstPart.Split(',');
                if (parts.Length < 2) return false;
                return double.TryParse(parts[0], out lat) && double.TryParse(parts[1], out lng);
            }
            catch { return false; }
        }

        // Reverse geocode using OpenStreetMap Nominatim
        private async Task<string?> ReverseGeocodeAsync(double lat, double lng)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Sector13Welfare/1.0 (+contact@example.com)");
                var url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&lon={lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return null;
                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                if (doc.RootElement.TryGetProperty("display_name", out var dn))
                {
                    return dn.GetString();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
