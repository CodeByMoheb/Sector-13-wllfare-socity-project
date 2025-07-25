using System;
using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Notice
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string CreatedBy { get; set; } // Manager username or id
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;
        public string? ApprovedBy { get; set; } // Secretary username or id
        public DateTime? ApprovedAt { get; set; }
    }
} 