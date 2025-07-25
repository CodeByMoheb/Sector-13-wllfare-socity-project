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

            var salarySheet = new List<SalarySheetViewModel>();
            foreach (var emp in employees)
            {
                // Count present days for this employee in the selected month
                var presentDays = attendances.Count(a => a.EmployeeId == emp.Id && a.IsPresent);
                decimal dailyWage = Math.Round(emp.BaseSalary / 30, 2); // Assuming 30 days in a month
                decimal total = Math.Round(dailyWage * presentDays, 2);
                decimal foodAllowance = 200; // Static for now, can be dynamic
                decimal staffLoan = 200; // Static for now, can be dynamic
                decimal netSalary = total + foodAllowance + staffLoan;

                salarySheet.Add(new SalarySheetViewModel
                {
                    EmployeeId = emp.Id,
                    Name = emp.Name,
                    Role = emp.Role,
                    JoiningDate = emp.JoiningDate,
                    BaseSalary = emp.BaseSalary,
                    DailyWage = dailyWage,
                    WorkingDays = presentDays,
                    Total = total,
                    FoodAllowance = foodAllowance,
                    StaffLoan = staffLoan,
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