using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ApprovalRequest
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string RequestType { get; set; } = string.Empty; // SalarySheet, Expense, Budget
        
        public decimal Amount { get; set; }
        
        public string RequestedBy { get; set; } = string.Empty; // User ID
        
        public string RequestedByName { get; set; } = string.Empty;
        
        public DateTime RequestDate { get; set; } = DateTime.Now;
        
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        
        public string? SecretaryApprovalBy { get; set; }
        public DateTime? SecretaryApprovalDate { get; set; }
        public string? SecretaryComments { get; set; }
        
        public string? PresidentApprovalBy { get; set; }
        public DateTime? PresidentApprovalDate { get; set; }
        public string? PresidentComments { get; set; }
        
        public string? RejectionReason { get; set; }
        public DateTime? RejectionDate { get; set; }
        public string? RejectedBy { get; set; }
    }
    
    public enum ApprovalStatus
    {
        Pending,
        SecretaryApproved,
        PresidentApproved,
        Approved,
        Rejected
    }
} 