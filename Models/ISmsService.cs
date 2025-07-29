namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<bool> SendBulkSmsAsync(List<string> phoneNumbers, string message);
        Task<bool> SendNoticeNotificationAsync(string noticeTitle, string noticeContent, List<string> memberPhoneNumbers);
        Task<bool> IsSmsEnabledAsync();
        Task<string> GetBalanceAsync();
    }
} 