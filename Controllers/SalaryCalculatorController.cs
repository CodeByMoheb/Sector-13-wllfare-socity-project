using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class SalaryCalculatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SalaryCalculatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var now = DateTime.Now;
            int selectedMonth = month ?? now.Month;
            int selectedYear = year ?? now.Year;

            // Get all active employees
            var employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
            // Get all attendance records for the selected month and year
            var attendances = await _context.Attendances
                .Where(a => a.Date.Month == selectedMonth && a.Date.Year == selectedYear)
                .ToListAsync();
            // Get approved leaves intersecting this month
            var monthStart = new DateTime(selectedYear, selectedMonth, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var approvedLeaves = await _context.Leaves
                .Where(l => l.ApprovalStatus == "Approved" &&
                            (l.StartDate <= monthEnd && l.EndDate >= monthStart))
                .ToListAsync();

            var salarySheet = new List<SalarySheetViewModel>();
            foreach (var emp in employees)
            {
                // Count present days for this employee in the selected month
                var presentDays = attendances.Count(a => a.EmployeeId == emp.Id &&
                    (a.Status == "Present" || a.Status == "On-time" || a.Status == "Late"));

                // Approved leave days count (treated as paid days)
                int approvedLeaveDays = 0;
                var empLeaves = approvedLeaves.Where(l => l.EmployeeId == emp.Id).ToList();
                foreach (var lv in empLeaves)
                {
                    var from = lv.StartDate.Date < monthStart ? monthStart : lv.StartDate.Date;
                    var to = lv.EndDate.Date > monthEnd ? monthEnd : lv.EndDate.Date;
                    if (to >= from)
                    {
                        approvedLeaveDays += (int)(to - from).TotalDays + 1;
                    }
                }

                var workingDays = presentDays + approvedLeaveDays;

                // Daily salary derived from base/30
                decimal dailyWage = Math.Round(emp.BaseSalary / 30m, 2);
                // Earned base based on attendance + approved leaves
                decimal earnedBase = Math.Round(dailyWage * workingDays, 2);

                // Allowances must be based on main base salary (not earned base)
                decimal house = Math.Round(emp.BaseSalary * 0.30m, 2);
                decimal medical = Math.Round(emp.BaseSalary * 0.05m, 2);
                decimal convey = Math.Round(emp.BaseSalary * 0.05m, 2);
                decimal gross40 = house + medical + convey;
                decimal netSalary = earnedBase + gross40;

                salarySheet.Add(new SalarySheetViewModel
                {
                    EmployeeId = emp.Id,
                    Name = emp.Name,
                    Role = emp.Role,
                    JoiningDate = emp.JoiningDate,
                    BaseSalary = emp.BaseSalary,
                    DailyWage = dailyWage,
                    WorkingDays = workingDays,
                    Total = earnedBase,
                    FoodAllowance = house + medical + convey, // using this field to carry total allowances for legacy compatibility
                    StaffLoan = gross40, // legacy column not shown in new UI but preserved
                    NetSalary = netSalary
                });
            }
            ViewBag.Month = selectedMonth;
            ViewBag.Year = selectedYear;
            ViewBag.NoEmployees = employees.Count == 0;
            ViewBag.NoAttendance = attendances.Count == 0;
            return View(salarySheet);
        }
    }
} 