using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
} 