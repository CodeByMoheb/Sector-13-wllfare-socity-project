using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = _configuration["EmailSettings:SmtpPort"];
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];

            // Check if email settings are configured
            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || 
                string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(senderEmail))
            {
                // In development, log the email details instead of sending
                Console.WriteLine($"=== EMAIL NOT SENT (Email not configured) ===");
                Console.WriteLine($"To: {to}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine($"Body: {body}");
                Console.WriteLine($"Reset Link: {body}");
                Console.WriteLine($"=============================================");
                return;
            }

            using (var client = new SmtpClient(smtpServer, int.Parse(smtpPort ?? "587")))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
            }
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Password Reset Request - Sector 13 Welfare Society";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h2 style='color: #1877f2;'>Sector 13 Welfare Society</h2>
                            <h3 style='color: #333;'>Password Reset Request</h3>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>
                            <p>Hello,</p>
                            <p>You have requested to reset your password for your account at Sector 13 Welfare Society Digital Management System.</p>
                            <p>Please click the button below to reset your password:</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' 
                                   style='background-color: #1877f2; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold;'>
                                    Reset Password
                                </a>
                            </div>
                            
                            <p>If the button doesn't work, you can copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; color: #1877f2;'>{resetLink}</p>
                            
                            <p><strong>Important:</strong></p>
                            <ul>
                                <li>This link will expire in 24 hours</li>
                                <li>If you didn't request this password reset, please ignore this email</li>
                                <li>For security reasons, this link can only be used once</li>
                            </ul>
                        </div>
                        
                        <div style='text-align: center; color: #666; font-size: 14px;'>
                            <p>Best regards,<br>Sector 13 Welfare Society Team</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmailWithAttachmentAsync(string to, string subject, string bodyHtml, byte[] attachmentBytes, string attachmentName, string contentType = "application/pdf")
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = _configuration["EmailSettings:SmtpPort"];
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || 
                string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(senderEmail))
            {
                // Log only in dev if not configured
                Console.WriteLine($"EMAIL WITH ATTACHMENT NOT SENT: {subject} to {to} (email not configured)");
                return;
            }

            using (var client = new SmtpClient(smtpServer, int.Parse(smtpPort ?? "587")))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                client.EnableSsl = true;

                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                };
                message.To.Add(to);
                var attachment = new Attachment(new MemoryStream(attachmentBytes), attachmentName, contentType);
                message.Attachments.Add(attachment);
                await client.SendMailAsync(message);
            }
        }
    }
} 