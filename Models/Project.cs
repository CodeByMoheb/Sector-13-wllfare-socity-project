using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Project
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public decimal RequiredAmount { get; set; }
        
        public decimal AllocatedAmount { get; set; } = 0;
        
        public decimal RemainingAmount => RequiredAmount - AllocatedAmount;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Planning"; // Planning, Active, Completed, Cancelled
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "General"; // General, Healthcare, Education, Emergency, Infrastructure, Environment
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        [StringLength(100)]
        public string? ProjectManager { get; set; }
        
        [StringLength(500)]
        public string? Objectives { get; set; }
        
        [StringLength(500)]
        public string? ExpectedOutcomes { get; set; }
        
        public bool IsPublic { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public DateTime? LastUpdated { get; set; }
        
        public string? LastUpdatedBy { get; set; }
        
        // Navigation properties
        public virtual ICollection<DonationAllocation> DonationAllocations { get; set; } = new List<DonationAllocation>();
        public virtual ICollection<ProjectProgress> ProjectProgresses { get; set; } = new List<ProjectProgress>();
    }
}
