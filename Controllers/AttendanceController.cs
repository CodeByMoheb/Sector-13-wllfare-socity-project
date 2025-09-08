using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            var employees = _context.Employees
                .Include(e => e.Shift)
                .Where(e => e.IsActive)
                .AsEnumerable();
            var attendance = _context.Attendances
                .Where(a => a.Date == targetDate)
                .AsEnumerable();
            ViewBag.Date = targetDate;
            return View((employees, attendance));
        }

        // GET: /Attendance/Manage - Manager's attendance management dashboard
        public async Task<IActionResult> Manage(DateTime? date, int? employeeId, string? status)
        {
            var targetDate = date ?? DateTime.Today;
            
            var query = _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e.Shift)
                .Where(a => a.Date == targetDate);

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            var attendances = await query.OrderBy(a => a.Employee.Name).ToListAsync();
            var employees = await _context.Employees.Where(e => e.IsActive).OrderBy(e => e.Name).ToListAsync();

            ViewBag.Date = targetDate;
            ViewBag.Employees = employees;
            ViewBag.SelectedEmployeeId = employeeId;
            ViewBag.SelectedStatus = status;

            return View(attendances);
        }

        // GET: /Attendance/AllRecords - View all attendance records
        public async Task<IActionResult> AllRecords(DateTime? from, DateTime? to, int? employeeId, string? status)
        {
            var startDate = from ?? DateTime.Today.AddMonths(-1);
            var endDate = to ?? DateTime.Today;

            var query = _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e.Shift)
                .Where(a => a.Date >= startDate && a.Date <= endDate);

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            var attendances = await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Employee.Name)
                .ToListAsync();

            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.Name)
                .ToListAsync();

            ViewBag.From = startDate;
            ViewBag.To = endDate;
            ViewBag.Employees = employees;
            ViewBag.SelectedEmployeeId = employeeId;
            ViewBag.SelectedStatus = status;

            return View(attendances);
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
                        Status = presentEmployeeIds.Contains(emp.Id) ? "Present" : "Absent",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Attendances.Add(att);
                }
                else
                {
                    att.Status = presentEmployeeIds.Contains(emp.Id) ? "Present" : "Absent";
                    att.UpdatedAt = DateTime.UtcNow;
                }
            }
            await _context.SaveChangesAsync();
            TempData["AttendanceSaved"] = "Attendance has been saved for " + date.ToString("dd MMM yyyy") + ".";
            return RedirectToAction("Index", new { date });
        }

        // POST: /Attendance/UpdateStatus - Update attendance status for specific employee
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int attendanceId, string status, string remarks = "")
        {
            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance == null)
            {
                return Json(new { success = false, message = "Attendance record not found" });
            }

            attendance.Status = status;
            attendance.Remarks = remarks;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Attendance status updated successfully" });
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
        public async Task<IActionResult> Edit(int employeeId, DateTime date, string status)
        {
            var attendance = _context.Attendances.FirstOrDefault(a => a.EmployeeId == employeeId && a.Date == date);
            if (attendance == null) return NotFound();
            attendance.Status = status;
            attendance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { date });
        }

        // GET: /Attendance/Report?employeeId={id}&from=yyyy-MM-dd&to=yyyy-MM-dd
        public IActionResult Report(int? employeeId, string? category, DateTime? from, DateTime? to)
        {
            var startDate = from ?? DateTime.Today.AddMonths(-1);
            var endDate = to ?? DateTime.Today;
            
            var query = _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e.Shift)
                .Where(a => a.Date >= startDate && a.Date <= endDate);

            if (employeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(a => a.Employee.Category == category);
            }

            var attendances = query.OrderByDescending(a => a.Date).ToList();
            var employees = _context.Employees.Where(e => e.IsActive).OrderBy(e => e.Name).ToList();
            var categories = _context.Employees.Where(e => e.IsActive).Select(e => e.Category).Distinct().OrderBy(c => c).ToList();

            ViewBag.From = startDate;
            ViewBag.To = endDate;
            ViewBag.Employees = employees;
            ViewBag.Categories = categories;
            ViewBag.SelectedEmployeeId = employeeId;
            ViewBag.SelectedCategory = category;

            return View(attendances);
        }

        // GET: /Attendance/EmployeeReport/{employeeId} - Individual employee attendance report
        public async Task<IActionResult> EmployeeReport(int employeeId, DateTime? from, DateTime? to)
        {
            var employee = await _context.Employees
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
                return NotFound();

            var startDate = from ?? DateTime.Today.AddMonths(-1);
            var endDate = to ?? DateTime.Today;

            var attendances = await _context.Attendances
                .Where(a => a.EmployeeId == employeeId && a.Date >= startDate && a.Date <= endDate)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            // Calculate statistics
            var totalDays = (endDate - startDate).Days + 1;
            var presentDays = attendances.Count(a => a.Status == "Present" || a.Status == "On-time" || a.Status == "Late");
            var absentDays = attendances.Count(a => a.Status == "Absent");
            var lateDays = attendances.Count(a => a.Status == "Late");
            var totalHours = attendances.Where(a => a.TotalHours.HasValue).Sum(a => a.TotalHours.Value);

            ViewBag.Employee = employee;
            ViewBag.From = startDate;
            ViewBag.To = endDate;
            ViewBag.TotalDays = totalDays;
            ViewBag.PresentDays = presentDays;
            ViewBag.AbsentDays = absentDays;
            ViewBag.LateDays = lateDays;
            ViewBag.TotalHours = totalHours;
            ViewBag.AttendancePercentage = totalDays > 0 ? Math.Round((double)presentDays / totalDays * 100, 2) : 0;

            return View(attendances);
        }

        // GET: /Attendance/LeaveManagement - Manage leave requests
        public async Task<IActionResult> LeaveManagement()
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

            ViewBag.PendingLeaves = pendingLeaves;
            ViewBag.ApprovedLeaves = approvedLeaves;

            return View();
        }

        // POST: /Attendance/ApproveLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLeave(int leaveId, string remarks = "")
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.LeaveId == leaveId);

            if (leave == null)
            {
                return Json(new { success = false, message = "Leave request not found" });
            }

            if (leave.ApprovalStatus != "Pending")
            {
                return Json(new { success = false, message = "Leave request is not pending approval" });
            }

            leave.ApprovalStatus = "Approved";
            leave.ApprovalRemarks = remarks;
            leave.ApprovalDate = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Leave request for {leave.Employee.Name} has been approved.";
            return Json(new { success = true, message = "Leave request approved successfully" });
        }

        // POST: /Attendance/RejectLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLeave(int leaveId, string remarks = "")
        {
            var leave = await _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.LeaveId == leaveId);

            if (leave == null)
            {
                return Json(new { success = false, message = "Leave request not found" });
            }

            if (leave.ApprovalStatus != "Pending")
            {
                return Json(new { success = false, message = "Leave request is not pending approval" });
            }

            leave.ApprovalStatus = "Rejected";
            leave.ApprovalRemarks = remarks;
            leave.ApprovalDate = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Leave request for {leave.Employee.Name} has been rejected.";
            return Json(new { success = true, message = "Leave request rejected successfully" });
        }

        // GET: /Attendance/SalaryCalculation - Calculate salary based on attendance
        public async Task<IActionResult> SalaryCalculation(int? employeeId, DateTime? month)
        {
            var targetMonth = month ?? DateTime.Today;
            var startDate = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var query = _context.Employees
                .Include(e => e.Shift)
                .Where(e => e.IsActive);

            if (employeeId.HasValue)
                query = query.Where(e => e.Id == employeeId.Value);

            var employees = await query.ToListAsync();
            var salaryData = new List<object>();

            foreach (var emp in employees)
            {
                var attendances = await _context.Leaves
                    .Where(a => a.EmployeeId == emp.Id && a.StartDate >= startDate && a.EndDate <= endDate)
                    .ToListAsync();

                var presentDays = attendances.Count(a => a.ApprovalStatus == "Approved" && 
                    (a.LeaveType == "Paid" || a.LeaveType == "Sick"));
                var unpaidLeaveDays = attendances.Count(a => a.ApprovalStatus == "Approved" && 
                    a.LeaveType == "Unpaid");

                // Calculate working days (assuming 22 working days per month)
                var totalWorkingDays = 22;
                var actualWorkingDays = totalWorkingDays - unpaidLeaveDays;
                var salary = emp.BaseSalary > 0 ? 
                    Math.Round((emp.BaseSalary / totalWorkingDays) * actualWorkingDays, 2) : 0;

                salaryData.Add(new
                {
                    EmployeeId = emp.EmployeeId,
                    Name = emp.Name,
                    BaseSalary = emp.BaseSalary,
                    PresentDays = presentDays,
                    UnpaidLeaveDays = unpaidLeaveDays,
                    ActualWorkingDays = actualWorkingDays,
                    CalculatedSalary = salary
                });
            }

            ViewBag.Month = targetMonth;
            ViewBag.Employees = employees;
            ViewBag.SelectedEmployeeId = employeeId;

            return View(salaryData);
        }

        // GET: /Attendance/ExportReport - Export attendance data
        public async Task<IActionResult> ExportReport(DateTime? from, DateTime? to, int? employeeId, string? status)
        {
            var startDate = from ?? DateTime.Today.AddMonths(-1);
            var endDate = to ?? DateTime.Today;

            var query = _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e.Shift)
                .Where(a => a.Date >= startDate && a.Date <= endDate);

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            var attendances = await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Employee.Name)
                .ToListAsync();

            // TODO: Implement Excel/PDF export
            // For now, return JSON
            return Json(attendances);
        }
    }
} 