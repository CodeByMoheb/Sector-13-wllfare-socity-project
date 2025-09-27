using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ProjectProgress
    {
        public int Id { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        
        [Required]
        public DateTime UpdateDate { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 100)]
        public int ProgressPercentage { get; set; }
        
        [Required]
        public decimal AmountUtilized { get; set; }
        
        [StringLength(200)]
        public string? Category { get; set; } // Milestone, Update, Issue, Completion
        
        [StringLength(100)]
        public string? Status { get; set; } = "Active"; // Active, Completed, On Hold, Cancelled
        
        [StringLength(500)]
        public string? Challenges { get; set; }
        
        [StringLength(500)]
        public string? NextSteps { get; set; }
        
        [StringLength(500)]
        public string? Images { get; set; } // JSON array of image URLs
        
        public string UpdatedBy { get; set; } = string.Empty;
        
        public bool IsPublic { get; set; } = true;
        
        // Navigation property
        public virtual Project Project { get; set; } = null!;
    }
}
