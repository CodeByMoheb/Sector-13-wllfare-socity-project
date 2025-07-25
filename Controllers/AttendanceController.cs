using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Attendance
        public IActionResult Index(DateTime? date)
        {
            var targetDate = date ?? DateTime.Today;
            var employees = _context.Employees.Where(e => e.IsActive).AsEnumerable();
            var attendance = _context.Attendances
                .Where(a => a.Date == targetDate)
                .AsEnumerable();
            ViewBag.Date = targetDate;
            return View((employees, attendance));
        }

        // POST: /Attendance/Mark
        [HttpPost]
        public async Task<IActionResult> Mark(DateTime date, int[] presentEmployeeIds)
        {
            var employees = _context.Employees.Where(e => e.IsActive).ToList();
            foreach (var emp in employees)
            {
                var att = _context.Attendances.FirstOrDefault(a => a.EmployeeId == emp.Id && a.Date == date);
                if (att == null)
                {
                    att = new Attendance
                    {
                        EmployeeId = emp.Id,
                        Date = date,
                        IsPresent = presentEmployeeIds.Contains(emp.Id)
                    };
                    _context.Attendances.Add(att);
                }
                else
                {
                    att.IsPresent = presentEmployeeIds.Contains(emp.Id);
                }
            }
            await _context.SaveChangesAsync();
            TempData["AttendanceSaved"] = "Attendance has been saved for " + date.ToString("dd MMM yyyy") + ".";
            return RedirectToAction("Index", new { date });
        }

        // GET: /Attendance/Edit/{employeeId}?date=yyyy-MM-dd
        public IActionResult Edit(int employeeId, DateTime date)
        {
            var attendance = _context.Attendances.FirstOrDefault(a => a.EmployeeId == employeeId && a.Date == date);
            if (attendance == null) return NotFound();
            var employee = _context.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null) return NotFound();
            ViewBag.Employee = employee;
            return View(attendance);
        }

        // POST: /Attendance/Edit/{employeeId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int employeeId, DateTime date, bool isPresent)
        {
            var attendance = _context.Attendances.FirstOrDefault(a => a.EmployeeId == employeeId && a.Date == date);
            if (attendance == null) return NotFound();
            attendance.IsPresent = isPresent;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { date });
        }

        // GET: /Attendance/Report?employeeId={id}&from=yyyy-MM-dd&to=yyyy-MM-dd
        public IActionResult Report(int? employeeId, DateTime? from, DateTime? to)
        {
            var startDate = from ?? DateTime.Today.AddMonths(-1);
            var endDate = to ?? DateTime.Today;
            var attendances = _context.Attendances
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .AsQueryable();
            if (employeeId.HasValue)
            {
                attendances = attendances.Where(a => a.EmployeeId == employeeId.Value);
            }
            var attendanceList = attendances.ToList();
            ViewBag.Employees = _context.Employees.ToList();
            ViewBag.EmployeeId = employeeId;
            ViewBag.From = startDate;
            ViewBag.To = endDate;
            return View(attendanceList);
        }
    }
} 