namespace Sector_13_Welfare_Society___Digital_Management_System.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
