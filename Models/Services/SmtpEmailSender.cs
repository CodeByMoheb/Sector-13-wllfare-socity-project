using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public SmtpEmailSender(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Welfare Society", _smtpSettings.UserName));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            // Body can be HTML or plain text
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();

            try
            {
                // Connect using STARTTLS on port 587
               await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
               await client.AuthenticateAsync("21103027@iubat.edu", "xvmkymavidjidwtl");


                // Send the email
                await client.SendAsync(message);

                // Disconnect cleanly
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                throw;
            }
        }
    }
}
