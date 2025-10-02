using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Admin,Secretary")]
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
            // Check for duplicate shift names
            if (await _context.Shifts.AnyAsync(s => s.Name == shift.Name && s.IsActive))
            {
                ModelState.AddModelError("Name", "A shift with this name already exists.");
            }

            // Validate time logic
            if (shift.StartTime == shift.EndTime)
            {
                ModelState.AddModelError("EndTime", "End time must be different from start time.");
            }

            if (ModelState.IsValid)
            {
                shift.CreatedAt = DateTime.UtcNow;
                shift.IsActive = true;
                _context.Shifts.Add(shift);
                await _context.SaveChangesAsync();
                TempData["ShiftCreated"] = $"Shift '{shift.Name}' created successfully.";
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

            // Check for duplicate shift names (excluding current shift)
            if (await _context.Shifts.AnyAsync(s => s.Name == shift.Name && s.IsActive && s.ShiftId != id))
            {
                ModelState.AddModelError("Name", "A shift with this name already exists.");
            }

            // Validate time logic
            if (shift.StartTime == shift.EndTime)
            {
                ModelState.AddModelError("EndTime", "End time must be different from start time.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    shift.UpdatedAt = DateTime.UtcNow;
                    _context.Shifts.Update(shift);
                    await _context.SaveChangesAsync();
                    TempData["ShiftUpdated"] = $"Shift '{shift.Name}' updated successfully.";
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

        // GET: /Shift/Templates
        public IActionResult Templates()
        {
            var templates = new List<object>
            {
                new { Name = "Morning Shift", StartTime = "08:00", EndTime = "16:00", Description = "8 AM to 4 PM" },
                new { Name = "Evening Shift", StartTime = "16:00", EndTime = "00:00", Description = "4 PM to 12 AM" },
                new { Name = "Night Shift", StartTime = "00:00", EndTime = "08:00", Description = "12 AM to 8 AM" },
                new { Name = "Day Shift", StartTime = "09:00", EndTime = "17:00", Description = "9 AM to 5 PM" },
                new { Name = "Part-time Morning", StartTime = "08:00", EndTime = "12:00", Description = "8 AM to 12 PM" },
                new { Name = "Part-time Afternoon", StartTime = "13:00", EndTime = "17:00", Description = "1 PM to 5 PM" }
            };
            return Json(templates);
        }

        // POST: /Shift/CreateFromTemplate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromTemplate(string templateName, string startTime, string endTime, string description)
        {
            if (TimeSpan.TryParse(startTime, out var start) && TimeSpan.TryParse(endTime, out var end))
            {
                var shift = new Shift
                {
                    Name = templateName,
                    StartTime = start,
                    EndTime = end,
                    Description = description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Shifts.Add(shift);
                await _context.SaveChangesAsync();
                TempData["ShiftCreated"] = $"Shift '{templateName}' created from template successfully.";
            }
            else
            {
                TempData["ShiftCreateError"] = "Invalid time format.";
            }

            return RedirectToAction("Index");
        }

        // GET: /Shift/BulkCreate
        public IActionResult BulkCreate()
        {
            return View();
        }

        // POST: /Shift/BulkCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(List<Shift> shifts)
        {
            var createdCount = 0;
            var errorCount = 0;

            foreach (var shift in shifts)
            {
                if (!string.IsNullOrEmpty(shift.Name))
                {
                    // Check for duplicates only
                    if (!await _context.Shifts.AnyAsync(s => s.Name == shift.Name && s.IsActive))
                    {
                        shift.CreatedAt = DateTime.UtcNow;
                        shift.IsActive = true;
                        _context.Shifts.Add(shift);
                        createdCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
            }

            if (createdCount > 0)
            {
                await _context.SaveChangesAsync();
                TempData["ShiftCreated"] = $"{createdCount} shifts created successfully.";
            }

            if (errorCount > 0)
            {
                TempData["ShiftCreateError"] = $"{errorCount} shifts could not be created due to duplicate names.";
            }

            return RedirectToAction("Index");
        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.ShiftId == id);
        }

    }
}
