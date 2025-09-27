using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class DonationAllocation
    {
        public int Id { get; set; }
        
        [Required]
        public int DonorId { get; set; }
        
        [Required]
        public int ProjectId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime AllocationDate { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Allocated"; // Allocated, Utilized, Refunded
        
        [StringLength(200)]
        public string? Purpose { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public string AllocatedBy { get; set; } = string.Empty; // User ID who made the allocation
        
        public DateTime? UtilizedDate { get; set; }
        
        [StringLength(500)]
        public string? UtilizationDetails { get; set; }
        
        public string? UtilizedBy { get; set; } // User ID who utilized the funds
        
        // Navigation properties
        public virtual Donor Donor { get; set; } = null!;
        public virtual Project Project { get; set; } = null!;
    }
}
