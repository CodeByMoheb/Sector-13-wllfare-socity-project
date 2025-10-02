using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Secretary,Admin")]
    public class LeaveApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeaveApprovalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Leave Approval Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== LEAVE APPROVAL INDEX LOADING ===");
                
                var pendingLeaves = await _context.Leaves
                    .Include(l => l.Employee)
                    .Where(l => l.ApprovalStatus == "Pending")
                    .OrderBy(l => l.CreatedAt)
                    .ToListAsync();

                var approvedLeaves = await _context.Leaves
                    .Include(l => l.Employee)
                    .Where(l => l.ApprovalStatus == "Approved")
                    .OrderByDescending(l => l.ApprovalDate)
                    .Take(10)
                    .ToListAsync();

                var rejectedLeaves = await _context.Leaves
                    .Include(l => l.Employee)
                    .Where(l => l.ApprovalStatus == "Rejected")
                    .OrderByDescending(l => l.ApprovalDate)
                    .Take(10)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Pending leaves count: {pendingLeaves?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Approved leaves count: {approvedLeaves?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Rejected leaves count: {rejectedLeaves?.Count ?? 0}");

                if (pendingLeaves != null && pendingLeaves.Any())
                {
                    foreach (var leave in pendingLeaves)
                    {
                        System.Diagnostics.Debug.WriteLine($"Pending leave: ID={leave.LeaveId}, Status={leave.ApprovalStatus}, Employee={leave.Employee?.Name}");
                    }
                }

                if (approvedLeaves != null && approvedLeaves.Any())
                {
                    foreach (var leave in approvedLeaves)
                    {
                        System.Diagnostics.Debug.WriteLine($"Approved leave: ID={leave.LeaveId}, Status={leave.ApprovalStatus}, Employee={leave.Employee?.Name}");
                    }
                }

                ViewBag.PendingLeaves = pendingLeaves ?? new List<Leave>();
                ViewBag.ApprovedLeaves = approvedLeaves ?? new List<Leave>();
                ViewBag.RejectedLeaves = rejectedLeaves ?? new List<Leave>();

                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LeaveApproval Index: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred while loading leave requests. Please try again.";
                
                // Return empty collections to prevent view errors
                ViewBag.PendingLeaves = new List<Leave>();
                ViewBag.ApprovedLeaves = new List<Leave>();
                ViewBag.RejectedLeaves = new List<Leave>();
                
                return View();
            }
        }

        // GET: Leave Details
        public async Task<IActionResult> Details(int id)
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .FirstOrDefaultAsync(l => l.LeaveId == id);

            if (leave == null)
            {
                return NotFound();
            }

            // Load current balances so manager sees the same numbers
            var currentYear = DateTime.Now.Year;
            var balances = await _context.LeaveBalances
                .Where(b => b.EmployeeId == leave.EmployeeId && b.Year == currentYear)
                .OrderBy(b => b.LeaveType)
                .ToListAsync();

            if (balances == null || balances.Count == 0)
            {
                // Compute balances dynamically from active policies + usage as fallback
                var policies = await _context.LeaveEntitlementPolicies
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.LeaveType)
                    .ToListAsync();

                var usedByType = await _context.Leaves
                    .Where(l => l.EmployeeId == leave.EmployeeId && l.ApprovalStatus == "Approved" && l.StartDate.Year == currentYear)
                    .GroupBy(l => l.LeaveType)
                    .Select(g => new { LeaveType = g.Key, Days = g.Sum(x => x.NumberOfDays) })
                    .ToDictionaryAsync(x => x.LeaveType, x => x.Days);

                var pendingByType = await _context.Leaves
                    .Where(l => l.EmployeeId == leave.EmployeeId && l.ApprovalStatus == "Pending" && l.StartDate.Year == currentYear)
                    .GroupBy(l => l.LeaveType)
                    .Select(g => new { LeaveType = g.Key, Days = g.Sum(x => x.NumberOfDays) })
                    .ToDictionaryAsync(x => x.LeaveType, x => x.Days);

                balances = policies.Select(p => new LeaveBalance
                {
                    EmployeeId = leave.EmployeeId,
                    Year = currentYear,
                    LeaveType = p.LeaveType,
                    TotalEntitled = p.DefaultEntitled,
                    Used = usedByType.ContainsKey(p.LeaveType) ? usedByType[p.LeaveType] : 0,
                    Pending = pendingByType.ContainsKey(p.LeaveType) ? pendingByType[p.LeaveType] : 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();
            }

            ViewBag.LeaveBalances = balances;
            return View(leave);
        }

        private async Task<(bool ok, string message)> ApproveInternal(int id, string remarks, ApplicationUser currentUser)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== APPROVE LEAVE REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"Leave ID: {id}, Remarks: {remarks}");
                
                var leave = await _context.Leaves
                    .Include(l => l.Employee)
                    .FirstOrDefaultAsync(l => l.LeaveId == id);

                if (leave == null)
                {
                    System.Diagnostics.Debug.WriteLine("Leave request not found");
                    return (false, "Leave request not found");
                }

                System.Diagnostics.Debug.WriteLine($"Found leave: ID={leave.LeaveId}, Status={leave.ApprovalStatus}, Employee={leave.Employee?.Name}");

                if (leave.ApprovalStatus != "Pending")
                {
                    System.Diagnostics.Debug.WriteLine($"Leave is not pending, current status: {leave.ApprovalStatus}");
                    return (false, "Leave request is not pending approval");
                }

                System.Diagnostics.Debug.WriteLine($"Current user: {currentUser.UserName}");

                // Update balances via service to move Pending -> Used and mark leave approved
                var service = HttpContext.RequestServices.GetService(typeof(ILeaveManagementService)) as ILeaveManagementService;
                if (service != null)
                {
                    var ok = await service.ApproveLeave(leave.LeaveId, currentUser.Id, remarks);
                    if (!ok)
                    {
                        // Fallback inline path (never fail approval if balances had issues)
                        var year = leave.StartDate.Year;
                        var balance = await _context.LeaveBalances
                            .FirstOrDefaultAsync(b => b.EmployeeId == leave.EmployeeId && b.Year == year && b.LeaveType.ToLower() == leave.LeaveType.ToLower());
                        if (balance == null)
                        {
                            // Create minimal balance row
                            var entitlement = await _context.LeaveEntitlementPolicies
                                .Where(p => p.IsActive && p.LeaveType.ToLower() == leave.LeaveType.ToLower())
                                .Select(p => p.DefaultEntitled)
                                .FirstOrDefaultAsync();
                            balance = new LeaveBalance
                            {
                                EmployeeId = leave.EmployeeId,
                                Year = year,
                                LeaveType = leave.LeaveType,
                                TotalEntitled = entitlement,
                                Used = 0,
                                Pending = 0,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.LeaveBalances.Add(balance);
                        }
                        var pendingToConsume = Math.Min(balance.Pending, leave.NumberOfDays);
                        balance.Pending -= pendingToConsume;
                        balance.Used += leave.NumberOfDays;
                        balance.UpdatedAt = DateTime.UtcNow;
                        _context.LeaveBalances.Update(balance);

                        leave.ApprovalStatus = "Approved";
                        leave.ApprovalRemarks = remarks;
                        leave.ApprovedById = currentUser.Id;
                        leave.ApprovalDate = DateTime.UtcNow;
                        leave.UpdatedAt = DateTime.UtcNow;
                        _context.Leaves.Update(leave);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Fallback if service is unavailable
                    leave.ApprovalStatus = "Approved";
                    leave.ApprovalRemarks = remarks;
                    leave.ApprovedById = currentUser.Id;
                    leave.ApprovalDate = DateTime.UtcNow;
                    leave.UpdatedAt = DateTime.UtcNow;
                    _context.Leaves.Update(leave);
                    await _context.SaveChangesAsync();
                }
                TempData["SuccessMessage"] = $"Leave request for {leave.Employee.Name} has been approved.";
                System.Diagnostics.Debug.WriteLine($"Approval successful for leave ID: {leave.LeaveId}");
                return (true, "Leave request approved successfully");
            }
            catch (Exception ex)
            {
                var msg = $"Approve error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(msg);
                if (ex.InnerException != null) System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.Message}");
                return (false, msg);
            }
        }

        // POST: Approve Leave (form submits)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string remarks = "")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Json(new { success = false, message = "User not found" });
            var res = await ApproveInternal(id, remarks, currentUser);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Content-Type"].ToString().Contains("application/json"))
                return Json(new { success = res.ok, message = res.message });
            if (res.ok) return RedirectToAction("Index");
            TempData["ErrorMessage"] = res.message;
            return RedirectToAction("Index");
        }

        // GET: Approve Leave (for direct URL testing /LeaveApproval/Approve?id=123)
        [HttpGet]
        [ActionName("Approve")]
        public async Task<IActionResult> ApproveQuery(int id, [FromQuery] string remarks)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Json(new { success = false, message = "User not found" });
            var res = await ApproveInternal(id, remarks ?? string.Empty, currentUser);
            return Json(new { success = res.ok, message = res.message });
        }

        // POST: Reject Leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string remarks = "")
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== REJECT LEAVE REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"Leave ID: {id}, Remarks: {remarks}");
                
                var leave = await _context.Leaves
                    .Include(l => l.Employee)
                    .FirstOrDefaultAsync(l => l.LeaveId == id);

                if (leave == null)
                {
                    System.Diagnostics.Debug.WriteLine("Leave request not found");
                    return Json(new { success = false, message = "Leave request not found" });
                }

                System.Diagnostics.Debug.WriteLine($"Found leave: ID={leave.LeaveId}, Status={leave.ApprovalStatus}, Employee={leave.Employee?.Name}");

                if (leave.ApprovalStatus != "Pending")
                {
                    System.Diagnostics.Debug.WriteLine($"Leave is not pending, current status: {leave.ApprovalStatus}");
                    return Json(new { success = false, message = "Leave request is not pending approval" });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("Current user not found");
                    return Json(new { success = false, message = "User not found" });
                }

                System.Diagnostics.Debug.WriteLine($"Current user: {currentUser.UserName}");

                // Move pending back and mark rejected using service
                var service = HttpContext.RequestServices.GetService(typeof(ILeaveManagementService)) as ILeaveManagementService;
                if (service != null)
                {
                    var ok = await service.RejectLeave(leave.LeaveId, currentUser.Id, remarks);
                    if (!ok)
                    {
                        return Json(new { success = false, message = "Could not update balances for rejection." });
                    }
                }
                else
                {
                    // Fallback minimal path
                    leave.ApprovalStatus = "Rejected";
                    leave.ApprovalRemarks = remarks;
                    leave.ApprovedById = currentUser.Id;
                    leave.ApprovalDate = DateTime.UtcNow;
                    leave.UpdatedAt = DateTime.UtcNow;
                    _context.Leaves.Update(leave);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Leave request for {leave.Employee.Name} has been rejected.";
                System.Diagnostics.Debug.WriteLine($"Rejection successful for leave ID: {leave.LeaveId}");
                
                // Check if this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                    Request.Headers["Content-Type"].ToString().Contains("application/json"))
                {
                    return Json(new { success = true, message = "Leave request rejected successfully" });
                }
                else
                {
                    // Regular form submission - redirect back to index with success message
                    TempData["SuccessMessage"] = $"Leave request for {leave.Employee.Name} has been rejected.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rejecting leave: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, message = "An error occurred while rejecting the leave request" });
            }
        }

        // GET: Test Database Connection
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== TESTING DATABASE CONNECTION ===");
                
                // Test basic connection
                var totalLeaves = await _context.Leaves.CountAsync();
                System.Diagnostics.Debug.WriteLine($"Total leaves in database: {totalLeaves}");
                
                // Test pending leaves
                var pendingCount = await _context.Leaves.CountAsync(l => l.ApprovalStatus == "Pending");
                System.Diagnostics.Debug.WriteLine($"Pending leaves count: {pendingCount}");
                
                // Test approved leaves
                var approvedCount = await _context.Leaves.CountAsync(l => l.ApprovalStatus == "Approved");
                System.Diagnostics.Debug.WriteLine($"Approved leaves count: {approvedCount}");
                
                // Test rejected leaves
                var rejectedCount = await _context.Leaves.CountAsync(l => l.ApprovalStatus == "Rejected");
                System.Diagnostics.Debug.WriteLine($"Rejected leaves count: {rejectedCount}");
                
                // Get all leaves with details
                var allLeaves = await _context.Leaves
                    .Include(l => l.Employee)
                    .ToListAsync();
                
                System.Diagnostics.Debug.WriteLine($"All leaves details:");
                foreach (var leave in allLeaves)
                {
                    System.Diagnostics.Debug.WriteLine($"  Leave ID: {leave.LeaveId}, Status: {leave.ApprovalStatus}, Employee: {leave.Employee?.Name}, Created: {leave.CreatedAt}");
                }
                
                return Json(new { 
                    success = true, 
                    totalLeaves = totalLeaves,
                    pendingCount = pendingCount,
                    approvedCount = approvedCount,
                    rejectedCount = rejectedCount,
                    message = "Database connection test successful"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database test error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { 
                    success = false, 
                    message = $"Database test failed: {ex.Message}" 
                });
            }
        }

        // GET: All Leave Requests (for reporting)
        public async Task<IActionResult> AllLeaves(DateTime? from, DateTime? to, string status = "", int? employeeId = null)
        {
            var query = _context.Leaves
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(l => l.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.EndDate <= to.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.ApprovalStatus == status);

            if (employeeId.HasValue)
                query = query.Where(l => l.EmployeeId == employeeId.Value);

            var leaves = await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            ViewBag.From = from ?? DateTime.Today.AddMonths(-1);
            ViewBag.To = to ?? DateTime.Today;
            ViewBag.Status = status;
            ViewBag.EmployeeId = employeeId;

            // Get employees for filter dropdown
            ViewBag.Employees = await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();

            return View(leaves);
        }

        // Leave History page (renders AllLeaves view explicitly)
        [HttpGet]
        public async Task<IActionResult> History(DateTime? from, DateTime? to, string status = "", int? employeeId = null)
        {
            var query = _context.Leaves
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(l => l.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.EndDate <= to.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.ApprovalStatus == status);

            if (employeeId.HasValue)
                query = query.Where(l => l.EmployeeId == employeeId.Value);

            var leaves = await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            ViewBag.From = from ?? DateTime.Today.AddMonths(-1);
            ViewBag.To = to ?? DateTime.Today;
            ViewBag.Status = status;
            ViewBag.EmployeeId = employeeId;

            ViewBag.Employees = await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();

            return View("AllLeaves", leaves);
        }

        // GET: Export Leave Data
        public async Task<IActionResult> ExportLeaves(DateTime? from, DateTime? to, string status = "")
        {
            var query = _context.Leaves
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .AsQueryable();

            if (from.HasValue)
                query = query.Where(l => l.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.EndDate <= to.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.ApprovalStatus == status);

            var leaves = await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            // TODO: Implement Excel/PDF export
            // For now, return JSON
            return Json(leaves);
        }
    }
}

