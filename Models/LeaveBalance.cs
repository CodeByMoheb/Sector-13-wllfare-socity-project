using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class LeaveBalance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [StringLength(50)]
        public string LeaveType { get; set; } = string.Empty;

        // Total entitled leave for the year
        [Required]
        public int TotalEntitled { get; set; }

        // Leave used so far
        public int Used { get; set; } = 0;

        // Leave pending approval
        public int Pending { get; set; } = 0;

        // Calculated remaining leave
        [NotMapped]
        public int Remaining => TotalEntitled - Used - Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public static class LeaveTypes
    {
        public const string Annual = "Annual Leave";
        public const string Sick = "Sick Leave";
        public const string Casual = "Casual Leave";
        public const string Maternity = "Maternity Leave";
        public const string Paternity = "Paternity Leave";
        public const string Emergency = "Emergency Leave";
        public const string Religious = "Religious Leave";

        public static List<string> GetAllTypes()
        {
            return new List<string>
            {
                Annual, Sick, Casual, Maternity, Paternity, Emergency, Religious
            };
        }

        public static Dictionary<string, int> GetDefaultEntitlements()
        {
            return new Dictionary<string, int>
            {
                { Annual, 21 },        // 21 days annual leave
                { Sick, 14 },          // 14 days sick leave
                { Casual, 10 },        // 10 days casual leave
                { Maternity, 112 },    // 16 weeks maternity leave
                { Paternity, 7 },      // 1 week paternity leave
                { Emergency, 5 },      // 5 days emergency leave
                { Religious, 3 }       // 3 days religious leave
            };
        }
    }
}
