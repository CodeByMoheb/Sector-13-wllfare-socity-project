using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ContributionTrackingViewModel
    {
        public int DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string DonorEmail { get; set; } = string.Empty;
        public string DonorPhone { get; set; } = string.Empty;
        public decimal TotalDonated { get; set; }
        public decimal TotalAllocated { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime LastDonationDate { get; set; }
        public List<DonationAllocationDetail> Allocations { get; set; } = new List<DonationAllocationDetail>();
    }

    public class DonationAllocationDetail
    {
        public int AllocationId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCategory { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime AllocationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public DateTime? UtilizedDate { get; set; }
    }

    public class ProjectFundingViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal RequiredAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal FundingPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ProjectManager { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = true;
        public List<ProjectDonorDetail> TopDonors { get; set; } = new List<ProjectDonorDetail>();
        public List<ProjectProgress> RecentProgress { get; set; } = new List<ProjectProgress>();
    }

    public class ProjectDonorDetail
    {
        public int DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime AllocationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DonationAllocationRequest
    {
        [Required]
        public int DonorId { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [StringLength(200)]
        public string? Purpose { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class ProjectCreationRequest
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Required amount must be greater than 0")]
        public decimal RequiredAmount { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "General";
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        [StringLength(100)]
        public string? ProjectManager { get; set; }
        
        [StringLength(500)]
        public string? Objectives { get; set; }
        
        [StringLength(500)]
        public string? ExpectedOutcomes { get; set; }
        
        public bool IsPublic { get; set; } = true;
    }

    public class ProjectProgressUpdate
    {
        [Required]
        public int ProjectId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 100)]
        public int ProgressPercentage { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal AmountUtilized { get; set; }
        
        [StringLength(200)]
        public string? Category { get; set; }
        
        [StringLength(100)]
        public string Status { get; set; } = "Active";
        
        [StringLength(500)]
        public string? Challenges { get; set; }
        
        [StringLength(500)]
        public string? NextSteps { get; set; }
        
        public bool IsPublic { get; set; } = true;
    }
}
