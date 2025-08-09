using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class NoticeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public NoticeController(ApplicationDbContext context)
        {
            _context = context;
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
                    
                    TempData["Success"] = "Notice approved successfully.";
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