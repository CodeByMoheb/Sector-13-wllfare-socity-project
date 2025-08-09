using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class PermanentMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Member Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Father's/Husband's Name")]
        public string FathersOrHusbandsName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Road Number")]
        public string RoadNo { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "House Number")]
        public string HouseNo { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Sector")]
        public string Sector { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        [Display(Name = "Mobile Phone Number")]
        [RegularExpression(@"^(\+880|880|0)?1[3-9]\d{8}$", ErrorMessage = "Please enter a valid Bangladeshi mobile number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        [Display(Name = "National ID")]
        public string? NationalId { get; set; }

        [Display(Name = "Membership Date")]
        [DataType(DataType.Date)]
        public DateTime MembershipDate { get; set; } = DateTime.Now;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
