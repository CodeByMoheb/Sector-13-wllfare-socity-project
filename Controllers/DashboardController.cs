using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Data;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            // Redirect to role-specific dashboard
            if (roles.Contains("Admin"))
                return RedirectToAction("Admin");
            else if (roles.Contains("President"))
                return RedirectToAction("President");
            else if (roles.Contains("Secretary"))
                return RedirectToAction("Secretary");
            else if (roles.Contains("Manager"))
                return RedirectToAction("Manager");
            else if (roles.Contains("Member"))
                return RedirectToAction("Member");
            else
                return RedirectToAction("Member"); // Default fallback
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "Admin";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";
            return View();
        }

        [Authorize(Roles = "President")]
        public async Task<IActionResult> President()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "President";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";
            return View();
        }

        [Authorize(Roles = "Secretary")]
        public async Task<IActionResult> Secretary()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "Secretary";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";
            return View();
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Manager()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "Manager";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";
            return View();
        }

        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Member()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.UserName = user.UserName;
            ViewBag.Role = roles.FirstOrDefault() ?? "Member";
            ViewBag.FullName = !string.IsNullOrEmpty(user.Name) ? user.Name : user.UserName;
            ViewBag.Address = !string.IsNullOrEmpty(user.HouseNo) ? $"House: {user.HouseNo}, Sector: {user.Sector}, Ward: {user.Ward}" : "Address not set";
            ViewBag.Phone = !string.IsNullOrEmpty(user.PhoneNumber) ? user.PhoneNumber : "Phone not set";
            ViewBag.Email = user.Email;
            ViewBag.ProfilePictureUrl = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : Url.Content("~/Photos/logo.png");
            ViewBag.LastLogin = user.LastLoginTime?.ToString("g") ?? "Never";

            // Expose whether this identity account is an employee login (EmployeeID as username)
            ViewBag.IsEmployeeUser = !string.IsNullOrWhiteSpace(user.UserName) && user.UserName.StartsWith("EMP", StringComparison.OrdinalIgnoreCase);
            ViewBag.EmployeeId = user.UserName;
            return View();
        }

        // GET: Leave Request Form
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> LeaveRequest()
        {
            System.Diagnostics.Debug.WriteLine($"=== LEAVE REQUEST GET METHOD ===");
            var employeeId = User.Identity?.Name;
            System.Diagnostics.Debug.WriteLine($"Employee ID: {employeeId}");
            
            if (string.IsNullOrEmpty(employeeId))
            {
                System.Diagnostics.Debug.WriteLine("No employee ID, redirecting to login");
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
            {
                System.Diagnostics.Debug.WriteLine("Employee not found, redirecting to login");
                return RedirectToAction("Login", "Account");
            }

            System.Diagnostics.Debug.WriteLine($"Employee found: {employee.EmployeeId}, Name: {employee.Name}");
            ViewBag.Employee = employee;
            return View();
        }

        // POST: Submit Leave Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> LeaveRequest(Leave leave)
        {
            System.Diagnostics.Debug.WriteLine($"=== LEAVE REQUEST SUBMISSION START ===");
            System.Diagnostics.Debug.WriteLine($"Leave request submitted: LeaveType={leave?.LeaveType}, StartDate={leave?.StartDate}, EndDate={leave?.EndDate}, NumberOfDays={leave?.NumberOfDays}");
            System.Diagnostics.Debug.WriteLine($"Model is null: {leave == null}");
            
            // Log all form data
            System.Diagnostics.Debug.WriteLine($"Request.Form data:");
            foreach (var key in Request.Form.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"  {key}: {Request.Form[key]}");
            }
            
            if (leave != null)
            {
                System.Diagnostics.Debug.WriteLine($"Leave object details:");
                System.Diagnostics.Debug.WriteLine($"- LeaveType: {leave.LeaveType}");
                System.Diagnostics.Debug.WriteLine($"- StartDate: {leave.StartDate}");
                System.Diagnostics.Debug.WriteLine($"- EndDate: {leave.EndDate}");
                System.Diagnostics.Debug.WriteLine($"- NumberOfDays: {leave.NumberOfDays}");
                System.Diagnostics.Debug.WriteLine($"- Reason: {leave.Reason}");
            }
            
            var employeeId = User.Identity?.Name;
            System.Diagnostics.Debug.WriteLine($"Employee ID from User.Identity.Name: {employeeId}");
            System.Diagnostics.Debug.WriteLine($"User is authenticated: {User.Identity?.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"User claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
            
            if (string.IsNullOrEmpty(employeeId))
            {
                System.Diagnostics.Debug.WriteLine("Leave request failed: No employee ID");
                TempData["ErrorMessage"] = "Please login first";
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            System.Diagnostics.Debug.WriteLine($"Employee lookup result: {employee != null}");
            if (employee != null)
            {
                System.Diagnostics.Debug.WriteLine($"Employee found: ID={employee.Id}, EmployeeId={employee.EmployeeId}, Name={employee.Name}");
            }

            if (employee == null)
            {
                System.Diagnostics.Debug.WriteLine("Employee not found in database");
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
                System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("ModelState errors:");
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        if (errors.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"  {key}: {string.Join(", ", errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                }
                
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

                    System.Diagnostics.Debug.WriteLine($"About to save leave request to database:");
                    System.Diagnostics.Debug.WriteLine($"- EmployeeId: {leave.EmployeeId}");
                    System.Diagnostics.Debug.WriteLine($"- LeaveType: {leave.LeaveType}");
                    System.Diagnostics.Debug.WriteLine($"- StartDate: {leave.StartDate}");
                    System.Diagnostics.Debug.WriteLine($"- EndDate: {leave.EndDate}");
                    System.Diagnostics.Debug.WriteLine($"- NumberOfDays: {leave.NumberOfDays}");
                    System.Diagnostics.Debug.WriteLine($"- Reason: {leave.Reason}");
                    System.Diagnostics.Debug.WriteLine($"- ApprovalStatus: {leave.ApprovalStatus}");

                    _context.Leaves.Add(leave);
                    System.Diagnostics.Debug.WriteLine($"Leave added to context, about to save changes...");
                    
                    var changes = await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Database save completed. Changes saved: {changes}");

                    TempData["SuccessMessage"] = "Leave request submitted successfully! Your request is pending approval.";
                    System.Diagnostics.Debug.WriteLine($"Redirecting to MyLeaves...");
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
                System.Diagnostics.Debug.WriteLine($"=== LEAVE REQUEST ERROR ===");
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                TempData["ErrorMessage"] = "An error occurred while submitting your leave request. Please try again.";
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Leave request error: {ex.Message}");
            }

            ViewBag.Employee = employee;
            return View(leave);
        }

        // GET: My Leave Requests
        [Authorize(Roles = "Member")]
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
        [Authorize(Roles = "Member")]
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

    }
} 