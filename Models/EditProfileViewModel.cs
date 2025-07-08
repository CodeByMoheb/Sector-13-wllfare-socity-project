using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class EditProfileViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string FathersOrHusbandsName { get; set; } = string.Empty;

        [StringLength(20)]
        public string HouseNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string FlatNo { get; set; } = string.Empty;

        [StringLength(20)]
        public string RoadNo { get; set; } = string.Empty;

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

        [Required]
        [Phone]
        [StringLength(20)]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        public IFormFile? ProfilePicture { get; set; }
        public string? ExistingProfilePictureUrl { get; set; }
    }
} 