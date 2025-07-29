namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class SmsSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = false;
        public string Provider { get; set; } = "BulkSmsBD"; // Default provider for Bangladesh
    }
} 