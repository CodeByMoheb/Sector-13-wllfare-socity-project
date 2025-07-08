using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string FathersOrHusbandsName { get; set; } = string.Empty;

        [StringLength(20)]
        public string HouseNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string Ward { get; set; } = string.Empty;

        [StringLength(20)]
        public string Holding { get; set; } = string.Empty;

        [StringLength(20)]
        public string Sector { get; set; } = string.Empty;

        [StringLength(100)]
        public string Profession { get; set; } = string.Empty;

        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        [StringLength(10)]
        public string BloodGroup { get; set; } = string.Empty;

        [StringLength(100)]
        public string EducationalQualification { get; set; } = string.Empty;

        public int NumberOfChildren { get; set; }

        [StringLength(20)]
        public string Telephone { get; set; } = string.Empty;

        [StringLength(20)]
        public string FlatNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string RoadNo { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public override string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public override string Email { get; set; }

        [StringLength(200)]
        public string ProfilePictureUrl { get; set; } = string.Empty;

        public DateTime? LastLoginTime { get; set; }
    }
} 