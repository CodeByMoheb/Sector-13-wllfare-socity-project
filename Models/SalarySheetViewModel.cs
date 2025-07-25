using System;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class SalarySheetViewModel
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public DateTime JoiningDate { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal DailyWage { get; set; }
        public int WorkingDays { get; set; }
        public decimal Total { get; set; }
        public decimal FoodAllowance { get; set; }
        public decimal StaffLoan { get; set; }
        public decimal NetSalary { get; set; }
    }
} 