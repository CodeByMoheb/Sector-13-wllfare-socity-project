using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendPasswordResetEmailAsync(string to, string resetLink);
        Task SendEmailWithAttachmentAsync(string to, string subject, string bodyHtml, byte[] attachmentBytes, string attachmentName, string contentType = "application/pdf");
    }
} 