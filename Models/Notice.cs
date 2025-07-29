using System;
using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Notice
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Notice title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Notice Title")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Notice content is required.")]
        [Display(Name = "Notice Content")]
        public string Content { get; set; } = string.Empty;
        
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; } = string.Empty;
        
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; } = false;
        
        [Display(Name = "Approved By")]
        public string? ApprovedBy { get; set; }
        
        [Display(Name = "Approved At")]
        public DateTime? ApprovedAt { get; set; }
    }
} 