using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class NoticeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISmsSender _smsSender;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public NoticeController(ApplicationDbContext context, ISmsSender smsSender, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _smsSender = smsSender;
            _emailService = emailService;
            _configuration = configuration;
        }

        // GET: /Notice/ManagerList
        [Authorize(Roles = "Manager")]
        public IActionResult ManagerList()
        {
            var notices = _context.Notices
                .Where(n => n.CreatedBy == User.Identity.Name)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
            return View(notices);
        }

        // GET: /Notice/Create
        [Authorize(Roles = "Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Notice/Create
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notice notice)
        {
            // Server-set fields should not participate in client validation
            ModelState.Remove(nameof(Notice.CreatedBy));
            ModelState.Remove(nameof(Notice.CreatedAt));
            ModelState.Remove(nameof(Notice.IsApproved));
            ModelState.Remove(nameof(Notice.ApprovedBy));
            ModelState.Remove(nameof(Notice.ApprovedAt));

            if (ModelState.IsValid)
            {
                try
                {
                    notice.CreatedBy = User.Identity.Name;
                    notice.CreatedAt = DateTime.Now;
                    notice.IsApproved = false;
                    notice.ApprovedBy = null;
                    notice.ApprovedAt = null;
                    
                    _context.Notices.Add(notice);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Notice created successfully and sent for approval.";
                    return RedirectToAction("ManagerList");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the notice. Please try again.");
                    return View(notice);
                }
            }
            return View(notice);
        }

        // GET: /Notice/SecretaryList
        [Authorize(Roles = "Secretary")]
        public IActionResult SecretaryList()
        {
            var notices = _context.Notices
                .Where(n => !n.IsApproved)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
            return View(notices);
        }

        // GET: /Notice/PublishedList
        [Authorize(Roles = "Secretary")]
        public IActionResult PublishedList()
        {
            var notices = _context.Notices
                .Where(n => n.IsApproved)
                .OrderByDescending(n => n.ApprovedAt)
                .ToList();
            return View(notices);
        }

        // POST: /Notice/Approve/{id}
        [HttpPost]
        [Authorize(Roles = "Secretary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var notice = await _context.Notices.FirstOrDefaultAsync(n => n.Id == id);
                if (notice != null && !notice.IsApproved)
                {
                    notice.IsApproved = true;
                    notice.ApprovedBy = User.Identity.Name;
                    notice.ApprovedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    
                    // Send notifications inline to avoid disposed scoped services
                    try
                    {
                        var shouldSendSms = _configuration.GetValue<bool>("SmsSettings:SendOnNoticePublish");
                        var smsBody = $"Notice: {notice.Title}\n{(notice.Content.Length > 120 ? notice.Content.Substring(0,120)+"..." : notice.Content)}";

                        if (shouldSendSms)
                        {
                            var phones = await _context.PermanentMembers.Where(m => m.IsActive && !string.IsNullOrEmpty(m.PhoneNumber))
                                .Select(m => m.PhoneNumber).ToArrayAsync();
                            if (phones.Length > 0)
                            {
                                await _smsSender.SendBulkAsync(phones, smsBody);
                            }
                        }

                        var emails = await _context.PermanentMembers.Where(m => m.IsActive && m.Email != null && m.Email != "")
                            .Select(m => m.Email!).ToListAsync();
                        foreach (var email in emails)
                        {
                            await _emailService.SendEmailAsync(email, $"New Notice Published: {notice.Title}", $"<p>{notice.Content}</p>");
                        }
                        TempData["Success"] = "Notice approved successfully and notifications attempted.";
                    }
                    catch
                    {
                        TempData["Warning"] = "Notice approved. Failed to send some notifications.";
                    }
                }
                else
                {
                    TempData["Error"] = "Notice not found or already approved.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while approving the notice.";
            }
            
            return RedirectToAction("SecretaryList");
        }

        // POST: /Notice/Delete/{id}
        [HttpPost]
        [Authorize(Roles = "Secretary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var notice = await _context.Notices.FirstOrDefaultAsync(n => n.Id == id);
                if (notice == null)
                {
                    TempData["Error"] = "Notice not found.";
                    return RedirectToAction("SecretaryList");
                }

                _context.Notices.Remove(notice);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Notice deleted successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while deleting the notice.";
            }

            return RedirectToAction("SecretaryList");
        }

        // GET: /Notice/PublicList - Public notice listing
        public async Task<IActionResult> PublicList()
        {
            var approvedNotices = await _context.Notices
                .Where(n => n.IsApproved)
                .OrderByDescending(n => n.ApprovedAt)
                .ToListAsync();
            
            return View(approvedNotices);
        }

        // GET: /Notice/Details/{id} - Public notice details
        public async Task<IActionResult> Details(int id)
        {
            var notice = await _context.Notices
                .FirstOrDefaultAsync(n => n.Id == id && n.IsApproved);
            
            if (notice == null)
            {
                return NotFound();
            }
            
            return View(notice);
        }

        // GET: /Notice/AllNotices - For admin to see all notices
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllNotices()
        {
            var allNotices = await _context.Notices
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            
            return View(allNotices);
        }
    }
} 