using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class SheetMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Member Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Has Phone Number")]
        public bool HasPhoneNumber => !string.IsNullOrEmpty(PhoneNumber);
    }
} 