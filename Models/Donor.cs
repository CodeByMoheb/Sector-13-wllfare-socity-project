using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Donor
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; } = string.Empty; // Manual, SSLCommerz
        
        public string? TransactionId { get; set; }
        
        public string? PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed
        
        public DateTime DonationDate { get; set; } = DateTime.Now;
        
        public string? Message { get; set; }
        
        public bool IsAnonymous { get; set; } = false;
        
        public string? DonationType { get; set; } = "General"; // General, Healthcare, Education, Emergency
        
        public string? ReceiptNumber { get; set; }
    }
} 