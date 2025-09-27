using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager")]
    public class LeavePolicyController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LeavePolicyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var policies = await _context.LeaveEntitlementPolicies
                .OrderBy(p => p.LeaveType)
                .ToListAsync();
            return View(policies);
        }

        public IActionResult Create()
        {
            return View(new LeaveEntitlementPolicy { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeaveEntitlementPolicy policy)
        {
            if (!ModelState.IsValid) return View(policy);
            policy.CreatedAt = DateTime.UtcNow;
            _context.LeaveEntitlementPolicies.Add(policy);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Leave policy added";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var policy = await _context.LeaveEntitlementPolicies.FindAsync(id);
            if (policy == null) return NotFound();
            return View(policy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LeaveEntitlementPolicy policy)
        {
            if (id != policy.Id) return NotFound();
            if (!ModelState.IsValid) return View(policy);
            var existing = await _context.LeaveEntitlementPolicies.FindAsync(id);
            if (existing == null) return NotFound();
            existing.LeaveType = policy.LeaveType;
            existing.DefaultEntitled = policy.DefaultEntitled;
            existing.CarryForwardEnabled = policy.CarryForwardEnabled;
            existing.MaxCarryForward = policy.MaxCarryForward;
            existing.IsActive = policy.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Leave policy updated";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var policy = await _context.LeaveEntitlementPolicies.FindAsync(id);
            if (policy == null) return NotFound();
            policy.IsActive = !policy.IsActive;
            policy.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

