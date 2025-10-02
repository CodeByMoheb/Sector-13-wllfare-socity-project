using System;
using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class LeaveRequestViewModel
    {
        [Required]
        [Display(Name = "Leave Type")]
        public required string LeaveType { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "Number of Days")]
        public int NumberOfDays { get; set; }

        [Required]
        [Display(Name = "Reason")]
        [StringLength(500)]
        public required string Reason { get; set; }

        // Leave balance information for display
        public List<LeaveBalance>? LeaveBalances { get; set; }
        public Dictionary<string, int>? AvailableBalances { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeId { get; set; }
    }
}
