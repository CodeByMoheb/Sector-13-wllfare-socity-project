using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
            var notices = _context.Notices.OrderByDescending(n => n.CreatedAt).ToList();
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
                notice.CreatedBy = User.Identity.Name;
                notice.CreatedAt = DateTime.Now;
                notice.IsApproved = false;
                _context.Notices.Add(notice);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManagerList");
            }
            return View(notice);
        }

        // GET: /Notice/SecretaryList
        [Authorize(Roles = "Secretary")]
        public IActionResult SecretaryList()
        {
            var notices = _context.Notices.Where(n => !n.IsApproved).OrderByDescending(n => n.CreatedAt).ToList();
            return View(notices);
        }

        // POST: /Notice/Approve/{id}
        [HttpPost]
        [Authorize(Roles = "Secretary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var notice = _context.Notices.FirstOrDefault(n => n.Id == id);
            if (notice != null && !notice.IsApproved)
            {
                notice.IsApproved = true;
                notice.ApprovedBy = User.Identity.Name;
                notice.ApprovedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("SecretaryList");
        }
    }
} 