using System;
using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class LeaveEntitlementPolicy
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string LeaveType { get; set; } = string.Empty;

        [Range(0, 365)]
        public int DefaultEntitled { get; set; }

        public bool CarryForwardEnabled { get; set; }

        [Range(0, 365)]
        public int? MaxCarryForward { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

