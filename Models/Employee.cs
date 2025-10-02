using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(20)]
        public string EmployeeId { get; set; } = string.Empty; // Unique Employee ID for login

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [StringLength(50)]
        public required string Role { get; set; }

        public int? RoleId { get; set; }

        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Legacy field used across views/controllers

        [StringLength(50)]
        public string Department { get; set; } = string.Empty; // Employee department (replaces Category)

        [Required]
        [DataType(DataType.Currency)]
        public decimal BaseSalary { get; set; }

        [ForeignKey("Shift")]
        public int? ShiftId { get; set; }
        public Shift? Shift { get; set; }

        [Required]
        public DateTime JoiningDate { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        // Password fields for employee login
        [StringLength(100)]
        public string? PasswordHash { get; set; }

        [StringLength(100)]
        public string? PasswordSalt { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
} 