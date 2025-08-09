using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Admin,Secretary")] 
    public class ManagerToolsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISmsSender _smsSender;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public ManagerToolsController(IConfiguration configuration, ISmsSender smsSender, IEmailService emailService, ApplicationDbContext context)
        {
            _configuration = configuration;
            _smsSender = smsSender;
            _emailService = emailService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var config = new
            {
                SmsApiKey = string.IsNullOrEmpty(_configuration["SmsSettings:ApiKey"]) ? "Missing" : "Configured",
                SmsSenderId = string.IsNullOrEmpty(_configuration["SmsSettings:SenderId"]) ? "Missing" : _configuration["SmsSettings:SenderId"],
                SmsOnPublish = _configuration["SmsSettings:SendOnNoticePublish"] ?? "false",
                EmailHost = string.IsNullOrEmpty(_configuration["EmailSettings:SmtpServer"]) ? "Missing" : _configuration["EmailSettings:SmtpServer"],
                EmailUser = string.IsNullOrEmpty(_configuration["EmailSettings:SmtpUsername"]) ? "Missing" : "Configured"
            };
            ViewBag.Config = config;
            ViewBag.ActiveMembers = await _context.PermanentMembers.CountAsync(m => m.IsActive);
            ViewBag.WithEmail = await _context.PermanentMembers.CountAsync(m => m.IsActive && m.Email != null && m.Email != "");
            ViewBag.WithPhone = await _context.PermanentMembers.CountAsync(m => m.IsActive && m.PhoneNumber != null && m.PhoneNumber != "");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestSms(string testNumber, string message)
        {
            if (string.IsNullOrWhiteSpace(testNumber) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Phone and message are required.";
                return RedirectToAction("Notifications");
            }
            var ok = await _smsSender.SendAsync(testNumber, message);
            TempData[ok ? "Success" : "Error"] = ok ? "Test SMS sent." : "Failed to send SMS.";
            return RedirectToAction("Notifications");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestEmail(string testEmail)
        {
            if (string.IsNullOrWhiteSpace(testEmail))
            {
                TempData["Error"] = "Email is required.";
                return RedirectToAction("Notifications");
            }
            try
            {
                await _emailService.SendEmailAsync(testEmail, "Test Email - Sector 13", "<p>This is a test email from Sector 13 system.</p>");
                TempData["Success"] = "Test email sent.";
            }
            catch
            {
                TempData["Error"] = "Failed to send email. Check SMTP settings.";
            }
            return RedirectToAction("Notifications");
        }
    }
}


