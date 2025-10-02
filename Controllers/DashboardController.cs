using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
<<<<<<< Updated upstream
=======
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Services;
>>>>>>> Stashed changes

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
<<<<<<< Updated upstream

        public DashboardController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
=======
        private readonly ApplicationDbContext _context;
        private readonly ILeaveManagementService _leaveService;
        private readonly IProjectManagementService _projectService;
        private readonly IDonationAllocationService _allocationService;

        public DashboardController(
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context, 
            ILeaveManagementService leaveService,
            IProjectManagementService projectService,
            IDonationAllocationService allocationService)
        {
            _userManager = userManager;
            _context = context;
            _leaveService = leaveService;
            _projectService = projectService;
            _allocationService = allocationService;
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return null;

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == appUser.UserName);
            if (employee != null) return employee;

            if (!string.IsNullOrEmpty(appUser.Email))
            {
                employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == appUser.Email);
                if (employee != null) return employee;
            }

            if (!string.IsNullOrEmpty(appUser.PhoneNumber))
            {
                employee = await _context.Employees.FirstOrDefaultAsync(e => e.Phone == appUser.PhoneNumber);
                if (employee != null) return employee;
            }

            return null;
>>>>>>> Stashed changes
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

            // Server-side metrics for cards
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var totalEmployees = await _context.Employees.CountAsync();
            var presentToday = await _context.Attendances
                .Where(a => a.Date == today && (a.Status == "Present" || a.Status == "On-time" || a.Status == "Late"))
                .CountAsync();
            var attendancePercentage = totalEmployees > 0 ? Math.Round(presentToday * 100.0 / totalEmployees, 0) : 0;

            var pendingLeaves = await _context.Leaves.Where(l => l.ApprovalStatus == "Pending").CountAsync();

            var monthlyDonations = await _context.Donors
                .Where(d => d.DonationDate >= startOfMonth && d.PaymentStatus == "Completed")
                .SumAsync(d => (decimal?)d.Amount) ?? 0m;

            var projects = await _projectService.GetAllProjectsAsync();
            var avgFundingPercentage = projects.Count > 0
                ? Math.Round(projects.Average(p => _projectService.CalculateProjectFundingPercentageAsync(p.Id).Result), 0)
                : 0;

            ViewBag.TotalEmployees = totalEmployees;
            ViewBag.PresentToday = presentToday;
            ViewBag.AttendancePercentage = attendancePercentage;
            ViewBag.PendingLeaves = pendingLeaves;
            ViewBag.MonthlyDonations = monthlyDonations;
            ViewBag.AvgFundingPercentage = avgFundingPercentage;

            // Weekly attendance data (Sun..Sat)
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var weekly = new List<object>();
            var weeklyLabels = new List<string>();
            var weeklyPercentages = new List<double>();
            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);
                var presentCount = await _context.Attendances
                    .Where(a => a.Date == date && (a.Status == "Present" || a.Status == "On-time" || a.Status == "Late"))
                    .CountAsync();
                var pct = totalEmployees > 0 ? Math.Round(presentCount * 100.0 / totalEmployees, 0) : 0;
                weekly.Add(new { day = date.ToString("ddd"), percentage = pct });
                weeklyLabels.Add(date.ToString("ddd"));
                weeklyPercentages.Add(pct);
            }
            ViewBag.WeeklyAttendance = weekly;
            ViewBag.WeeklyLabels = weeklyLabels;
            ViewBag.WeeklyPercentages = weeklyPercentages;

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
<<<<<<< Updated upstream
            return View();
        }
=======

            // Expose whether this identity account is an employee login (EmployeeID as username)
            ViewBag.IsEmployeeUser = !string.IsNullOrWhiteSpace(user.UserName) && user.UserName.StartsWith("EMP", StringComparison.OrdinalIgnoreCase);
            ViewBag.EmployeeId = user.UserName;
            
            // If this is an employee user, get employee-specific data
            if (ViewBag.IsEmployeeUser)
            {
                var employee = await _context.Employees
                    .Include(e => e.Shift)
                    .FirstOrDefaultAsync(e => e.EmployeeId == user.UserName);

                if (employee != null)
                {
                    // Get today's attendance
                    var todaysAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date.Date == DateTime.Today);

                    // Get recent attendance (last 7 days)
                    var recentAttendances = await _context.Attendances
                        .Where(a => a.EmployeeId == employee.Id && a.Date >= DateTime.Today.AddDays(-7))
                        .OrderByDescending(a => a.Date)
                        .ToListAsync();

                    // Calculate this month's statistics
                    var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    var monthAttendances = await _context.Attendances
                        .Where(a => a.EmployeeId == employee.Id && a.Date >= monthStart)
                        .ToListAsync();

                    ViewBag.Employee = employee;
                    ViewBag.TodaysAttendance = todaysAttendance;
                    ViewBag.RecentAttendances = recentAttendances;
                    ViewBag.MonthlyPresentDays = monthAttendances.Count(a => a.Status == "Present");
                    ViewBag.MonthlyAbsentDays = monthAttendances.Count(a => a.Status == "Absent");
                    ViewBag.MonthlyLateDays = monthAttendances.Count(a => a.Status == "Late");
                    ViewBag.TotalWorkingDays = DateTime.Today.Day;
                    
                    // Override user info with employee info
                    ViewBag.FullName = employee.Name;
                    ViewBag.Phone = employee.Phone ?? "Phone not set";
                    ViewBag.Address = employee.Address ?? "Address not set";
                }
            }
            
            return View();
        }

        // GET: Leave Request Form
        [Authorize(Roles = "Member")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> LeaveRequest()
        {
            var employee = await GetCurrentEmployeeAsync();

            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentYear = DateTime.Now.Year;
            
            List<LeaveBalance> leaveBalances;
            try
            {
                // Get employee leave balances
                leaveBalances = await _leaveService.GetEmployeeLeaveBalances(employee.Id, currentYear);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading leave balances: {ex.Message}");
                leaveBalances = new List<LeaveBalance>();
                TempData["Warning"] = "Leave balances could not be loaded. Please contact administrator.";
            }
            
            var viewModel = new LeaveRequestViewModel
            {
                LeaveType = "",
                Reason = "",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                LeaveBalances = leaveBalances,
                AvailableBalances = leaveBalances?.ToDictionary(lb => lb.LeaveType, lb => lb.Remaining) ?? new Dictionary<string, int>(),
                EmployeeName = employee.Name,
                EmployeeId = employee.EmployeeId
            };

            ViewBag.Employee = employee;
            ViewBag.LeaveTypes = await _context.LeaveEntitlementPolicies
                .Where(p => p.IsActive)
                .OrderBy(p => p.LeaveType)
                .Select(p => p.LeaveType)
                .ToListAsync();
            
            return View(viewModel);
        }

        // POST: Submit Leave Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> LeaveRequest(LeaveRequestViewModel model)
        {
            System.Diagnostics.Debug.WriteLine($"[LeaveRequest] Starting leave request submission for user: {User.Identity?.Name}");
            
            try
            {
                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("[LeaveRequest] Model validation failed:");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        System.Diagnostics.Debug.WriteLine($"  - Validation error: {error.ErrorMessage}");
                    }
                    
                    // Re-populate the view model with leave balances
                    await PopulateLeaveRequestViewModel(model);
                    return View(model);
                }

                var employee = await GetCurrentEmployeeAsync();

                if (employee == null)
                {
                    System.Diagnostics.Debug.WriteLine("[LeaveRequest] ERROR: Employee not found for current user");
                    TempData["ErrorMessage"] = "Employee record not found. Please contact administrator.";
                    return RedirectToAction("Login", "Account");
                }

                System.Diagnostics.Debug.WriteLine($"[LeaveRequest] Employee found: {employee.Name} (ID: {employee.Id}, EmployeeId: {employee.EmployeeId})");

                // Calculate number of days if not provided
                if (model.NumberOfDays <= 0)
                {
                    model.NumberOfDays = (int)(model.EndDate - model.StartDate).TotalDays + 1;
                    System.Diagnostics.Debug.WriteLine($"[LeaveRequest] Auto-calculated days: {model.NumberOfDays}");
                }
                else
                {
                    // If user entered days manually, still validate against dates when both dates present
                    if (model.StartDate != default && model.EndDate != default)
                    {
                        var calc = (int)(model.EndDate - model.StartDate).TotalDays + 1;
                        if (calc > 0 && model.NumberOfDays != calc)
                        {
                            model.NumberOfDays = calc;
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[LeaveRequest] Leave details - Type: {model.LeaveType}, Days: {model.NumberOfDays}, Start: {model.StartDate:yyyy-MM-dd}, End: {model.EndDate:yyyy-MM-dd}");

                // Validate dates
                if (model.StartDate < DateTime.Today)
                {
                    System.Diagnostics.Debug.WriteLine("[LeaveRequest] ERROR: Start date is in the past");
                    ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
                    await PopulateLeaveRequestViewModel(model);
                    return View(model);
                }

                if (model.EndDate < model.StartDate)
                {
                    System.Diagnostics.Debug.WriteLine("[LeaveRequest] ERROR: End date is before start date");
                    ModelState.AddModelError("EndDate", "End date cannot be before start date.");
                    await PopulateLeaveRequestViewModel(model);
                    return View(model);
                }

                // Check if employee has sufficient leave balance
                System.Diagnostics.Debug.WriteLine($"[LeaveRequest] Checking leave balance for employee {employee.Id}, leave type: {model.LeaveType}, days: {model.NumberOfDays}");
                
                var canApply = await _leaveService.CanApplyForLeave(employee.Id, model.LeaveType, model.NumberOfDays);
                System.Diagnostics.Debug.WriteLine($"[LeaveRequest] Can apply for leave: {canApply}");
                
                if (!canApply)
                {
                    var balance = await _leaveService.GetLeaveBalance(employee.Id, DateTime.Now.Year, model.LeaveType);
                    var remaining = balance?.Remaining ?? 0;
                    System.Diagnostics.Debug.WriteLine($"[LeaveRequest] ERROR: Insufficient balance. Remaining: {remaining}, Requested: {model.NumberOfDays}");
                    
                    ModelState.AddModelError("", $"Insufficient {model.LeaveType} balance. You have {remaining} days remaining but requested {model.NumberOfDays} days.");
                    await PopulateLeaveRequestViewModel(model);
                    return View(model);
                }

                // Create leave request
                var leave = new Leave
                {
                    EmployeeId = employee.Id,
                    LeaveType = model.LeaveType,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    NumberOfDays = model.NumberOfDays,
                    Reason = model.Reason,
                    ApprovalStatus = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                System.Diagnostics.Debug.WriteLine($"[LeaveRequest] Created leave object: EmployeeId={leave.EmployeeId}, LeaveType={leave.LeaveType}, Days={leave.NumberOfDays}");

                // Apply for leave using the service
                System.Diagnostics.Debug.WriteLine("[LeaveRequest] Calling leave service to apply for leave...");
                var success = await _leaveService.ApplyForLeave(leave);
                System.Diagnostics.Debug.WriteLine($"[LeaveRequest] Leave service result: {success}");

                if (success)
                {
                    System.Diagnostics.Debug.WriteLine("[LeaveRequest] SUCCESS: Leave request submitted successfully");
                    TempData["LeaveSuccess"] = $"Leave request submitted successfully! Your {model.LeaveType} request for {model.NumberOfDays} days is pending approval.";
                    return RedirectToAction("LeaveRequest");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[LeaveRequest] ERROR: Leave service returned false");
                    ModelState.AddModelError("", "Failed to submit leave request. Please check your leave balance and try again.");
                    await PopulateLeaveRequestViewModel(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LeaveRequest] EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LeaveRequest] Stack trace: {ex.StackTrace}");
                
                ModelState.AddModelError("", $"An error occurred while submitting your leave request: {ex.Message}. Please try again or contact support.");
                
                try
                {
                    await PopulateLeaveRequestViewModel(model);
                }
                catch (Exception populateEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[LeaveRequest] ERROR populating view model: {populateEx.Message}");
                }
                
                return View(model);
            }
        }

        private async Task PopulateLeaveRequestViewModel(LeaveRequestViewModel model)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee != null)
            {
                    try
                    {
                        var currentYear = DateTime.Now.Year;
                        var leaveBalances = await _leaveService.GetEmployeeLeaveBalances(employee.Id, currentYear);
                        
                        model.LeaveBalances = leaveBalances;
                        model.AvailableBalances = leaveBalances?.ToDictionary(lb => lb.LeaveType, lb => lb.Remaining) ?? new Dictionary<string, int>();
                        model.EmployeeName = employee.Name;
                        model.EmployeeId = employee.EmployeeId;
                        
                        ViewBag.Employee = employee;
                    }
                    catch (Exception ex)
                    {
                        // Log the error and provide fallback values
                        System.Diagnostics.Debug.WriteLine($"Error loading leave balances: {ex.Message}");
                        
                        // Provide default empty balances
                        model.LeaveBalances = new List<LeaveBalance>();
                        model.AvailableBalances = new Dictionary<string, int>();
                        model.EmployeeName = employee.Name;
                        model.EmployeeId = employee.EmployeeId;
                        
                        ViewBag.Employee = employee;
                        TempData["Warning"] = "Leave balances could not be loaded. Please contact administrator.";
                    }
            }
            
            ViewBag.LeaveTypes = await _context.LeaveEntitlementPolicies
                .Where(p => p.IsActive)
                .OrderBy(p => p.LeaveType)
                .Select(p => p.LeaveType)
                .ToListAsync();
        }

        // GET: My Leave Balances (visible to all employees)
        [Authorize(Roles = "Member")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> LeaveBalances()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var currentYear = DateTime.Now.Year;
            var leaveBalances = await _leaveService.GetEmployeeLeaveBalances(employee.Id, currentYear);
            ViewBag.Employee = employee;
            ViewBag.Year = currentYear;
            return View(leaveBalances);
        }

        // Member Attendance Mark (for unified login system)
        [HttpPost]
        [Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkMemberAttendance(string action)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.UserName.StartsWith("EMP", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "Only employees can mark attendance." });
            }

            var employee = await _context.Employees
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(e => e.EmployeeId == user.UserName);

            if (employee == null || !employee.IsActive)
            {
                return Json(new { success = false, message = "Employee account not found or inactive." });
            }

            var today = DateTime.Today;
            var currentTime = DateTime.Now;

            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date.Date == today);

            if (action.ToLower() == "checkin")
            {
                if (existingAttendance != null)
                {
                    return Json(new { success = false, message = "You have already checked in today." });
                }

                var attendance = new Attendance
                {
                    EmployeeId = employee.Id,
                    Date = today,
                    CheckInTime = currentTime,
                    Status = DetermineAttendanceStatus(currentTime, employee.Shift)
                };

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Check-in successful at {currentTime:HH:mm}",
                    status = attendance.Status,
                    checkInTime = currentTime.ToString("HH:mm")
                });
            }
            else if (action.ToLower() == "checkout")
            {
                if (existingAttendance == null)
                {
                    return Json(new { success = false, message = "Please check in first." });
                }

                if (existingAttendance.CheckOutTime.HasValue)
                {
                    return Json(new { success = false, message = "You have already checked out today." });
                }

                existingAttendance.CheckOutTime = currentTime;
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Check-out successful at {currentTime:HH:mm}",
                    checkOutTime = currentTime.ToString("HH:mm")
                });
            }

            return Json(new { success = false, message = "Invalid action." });
        }

        private string DetermineAttendanceStatus(DateTime checkInTime, Shift? shift)
        {
            if (shift == null)
            {
                // If no shift assigned, consider late if after 9:00 AM
                return checkInTime.TimeOfDay <= new TimeSpan(9, 0, 0) ? "Present" : "Late";
            }

            // Compare with shift start time
            return checkInTime.TimeOfDay <= shift.StartTime ? "Present" : "Late";
        }

        [Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> ProjectManagementDashboard()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> GetProjectStats()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();
                var allocations = await _allocationService.GetAllDonorContributionsAsync();

                var stats = new
                {
                    totalProjects = projects.Count,
                    activeProjects = projects.Count(p => p.Status == "Active"),
                    completedProjects = projects.Count(p => p.Status == "Completed"),
                    totalRaised = projects.Sum(p => p.AllocatedAmount),
                    totalAllocations = allocations.Count(),
                    pendingAllocations = projects.Sum(p => p.RemainingAmount)
                };

                return Json(new { success = true, totalProjects = stats.totalProjects, activeProjects = stats.activeProjects, completedProjects = stats.completedProjects, totalRaised = stats.totalRaised, totalAllocations = stats.totalAllocations, pendingAllocations = stats.pendingAllocations });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> GetRecentProjects()
        {
            try
            {
                var projects = await _projectService.GetAllProjectFundingDetailsAsync();
                var recentProjects = projects
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(5)
                    .Select(p => new
                    {
                        id = p.ProjectId,
                        name = p.ProjectName,
                        category = p.ProjectCategory,
                        status = p.Status,
                        fundingPercentage = p.FundingPercentage,
                        allocatedAmount = p.AllocatedAmount,
                        requiredAmount = p.RequiredAmount
                    })
                    .ToList();

                return Json(new { success = true, projects = recentProjects });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> GetProjectCategories()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();
                var categories = projects
                    .GroupBy(p => p.Category)
                    .Select(g => new
                    {
                        name = g.Key,
                        count = g.Count(),
                        amount = g.Sum(p => p.AllocatedAmount)
                    })
                    .OrderByDescending(c => c.count)
                    .ToList();

                var totalProjects = projects.Count;

                return Json(new { success = true, categories = categories, totalProjects = totalProjects });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> GetRecentAllocations()
        {
            try
            {
                var allocations = await _context.DonationAllocations
                    .Include(da => da.Donor)
                    .Include(da => da.Project)
                    .Where(da => da.Donor != null && da.Project != null)
                    .OrderByDescending(da => da.AllocationDate)
                    .Take(5)
                    .Select(da => new
                    {
                        id = da.Id,
                        donorName = da.Donor!.Name,
                        projectName = da.Project.Name,
                        amount = da.Amount,
                        status = da.Status,
                        allocationDate = da.AllocationDate,
                        purpose = da.Purpose
                    })
                    .ToListAsync();

                return Json(new { success = true, allocations = allocations });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            try
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);

                // Attendance metrics
                var totalEmployees = await _context.Employees.CountAsync();
                var todayAttendance = await _context.Attendances
                    .Where(a => a.Date == today && a.Status == "Present")
                    .CountAsync();
                var attendancePercentage = totalEmployees > 0 ? (todayAttendance * 100.0 / totalEmployees) : 0;

                // Leave metrics
                var pendingLeaves = await _context.Leaves
                    .Where(l => l.ApprovalStatus == "Pending")
                    .CountAsync();

                // Donation metrics
                var monthlyDonations = await _context.Donors
                    .Where(d => d.DonationDate >= startOfMonth && d.PaymentStatus == "Completed")
                    .SumAsync(d => d.Amount);

                // Project metrics
                var projects = await _projectService.GetAllProjectsAsync();
                var activeProjects = projects.Count(p => p.Status == "Active");
                var avgFundingPercentage = projects.Count > 0 ? 
                    projects.Average(p => _projectService.CalculateProjectFundingPercentageAsync(p.Id).Result) : 0;

                var metrics = new
                {
                    totalEmployees = totalEmployees,
                    attendancePercentage = Math.Round(attendancePercentage, 1),
                    todayAttendance = todayAttendance,
                    pendingLeaves = pendingLeaves,
                    monthlyDonations = monthlyDonations,
                    activeProjects = activeProjects,
                    avgFundingPercentage = Math.Round(avgFundingPercentage, 1)
                };

                return Json(new { success = true, metrics = metrics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> GetWeeklyAttendanceData()
        {
            try
            {
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var attendanceData = new List<object>();

                for (int i = 0; i < 7; i++)
                {
                    var date = startOfWeek.AddDays(i);
                    var totalEmployees = await _context.Employees.CountAsync();
                    var presentCount = await _context.Attendances
                        .Where(a => a.Date == date && a.Status == "Present")
                        .CountAsync();
                    var percentage = totalEmployees > 0 ? (presentCount * 100.0 / totalEmployees) : 0;

                    attendanceData.Add(new
                    {
                        day = date.ToString("ddd"),
                        percentage = Math.Round(percentage, 1)
                    });
                }

                return Json(new { success = true, data = attendanceData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> GetRecentActivities()
        {
            try
            {
                var activities = new List<object>();
                return Json(new { success = true, activities = activities });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
>>>>>>> Stashed changes
    }
}
