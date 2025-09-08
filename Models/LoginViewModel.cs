using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or Employee ID is required")]
        [Display(Name = "Email or Employee ID")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
      
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
} 