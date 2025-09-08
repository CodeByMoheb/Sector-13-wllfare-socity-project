using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class ShiftController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShiftController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Shift
        public async Task<IActionResult> Index()
        {
            var shifts = await _context.Shifts
                .Where(s => s.IsActive)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
            return View(shifts);
        }

        // GET: /Shift/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Shift/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shift shift)
        {
            if (ModelState.IsValid)
            {
                shift.CreatedAt = DateTime.UtcNow;
                _context.Shifts.Add(shift);
                await _context.SaveChangesAsync();
                TempData["ShiftCreated"] = "Shift created successfully.";
                return RedirectToAction("Index");
            }
            return View(shift);
        }

        // GET: /Shift/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }
            return View(shift);
        }

        // POST: /Shift/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shift shift)
        {
            if (id != shift.ShiftId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    shift.UpdatedAt = DateTime.UtcNow;
                    _context.Shifts.Update(shift);
                    await _context.SaveChangesAsync();
                    TempData["ShiftUpdated"] = "Shift updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftExists(shift.ShiftId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(shift);
        }

        // POST: /Shift/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }

            // Check if any employees are assigned to this shift
            var employeesUsingShift = await _context.Employees
                .AnyAsync(e => e.ShiftId == id);

            if (employeesUsingShift)
            {
                TempData["ShiftDeleteError"] = "Cannot delete shift. Employees are assigned to this shift.";
                return RedirectToAction("Index");
            }

            shift.IsActive = false;
            shift.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["ShiftDeleted"] = "Shift deleted successfully.";
            return RedirectToAction("Index");
        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ShiftId == id);
        }
    }
}
