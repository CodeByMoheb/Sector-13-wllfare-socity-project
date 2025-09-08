using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
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

            ViewBag.PendingLeaves = pendingLeaves;
            ViewBag.ApprovedLeaves = approvedLeaves;
            ViewBag.RejectedLeaves = rejectedLeaves;

            return View();
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

            return View(leave);
        }

        // POST: Approve Leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string remarks = "")
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.LeaveId == id);

            if (leave == null)
            {
                return Json(new { success = false, message = "Leave request not found" });
            }

            if (leave.ApprovalStatus != "Pending")
            {
                return Json(new { success = false, message = "Leave request is not pending approval" });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            leave.ApprovalStatus = "Approved";
            leave.ApprovalRemarks = remarks;
            leave.ApprovedById = currentUser.Id;
            leave.ApprovalDate = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Leave request for {leave.Employee.Name} has been approved.";
            return Json(new { success = true, message = "Leave request approved successfully" });
        }

        // POST: Reject Leave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string remarks = "")
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.LeaveId == id);

            if (leave == null)
            {
                return Json(new { success = false, message = "Leave request not found" });
            }

            if (leave.ApprovalStatus != "Pending")
            {
                return Json(new { success = false, message = "Leave request is not pending approval" });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            leave.ApprovalStatus = "Rejected";
            leave.ApprovalRemarks = remarks;
            leave.ApprovedById = currentUser.Id;
            leave.ApprovalDate = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Leave request for {leave.Employee.Name} has been rejected.";
            return Json(new { success = true, message = "Leave request rejected successfully" });
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

