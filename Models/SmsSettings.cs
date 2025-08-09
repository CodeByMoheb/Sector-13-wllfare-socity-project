namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class SmsSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://bulksmsbd.net/api/smsapi";
        public string BalanceUrl { get; set; } = "https://bulksmsbd.net/api/getBalanceApi";
        public bool SendOnNoticePublish { get; set; } = true;
    }
}


