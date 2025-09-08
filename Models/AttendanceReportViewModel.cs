using System;
using System.Collections.Generic;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class AttendanceReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? EmployeeId { get; set; }
        public string? Category { get; set; }
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<Attendance> Attendances { get; set; } = new List<Attendance>();
        public List<string> Categories { get; set; } = new List<string>();
    }
}
