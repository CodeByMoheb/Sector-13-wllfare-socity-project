using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
<<<<<<< Updated upstream
=======
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
>>>>>>> Stashed changes

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        

// In Controllers/HomeController.cs
        public IActionResult Gallery()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public async Task<IActionResult> MessageOfChairman()
        {
            var leadershipMessages = await _context.LeadershipMessages
                .Where(m => m.IsActive && (m.MessageType == "President" || m.MessageType == "VicePresident"))
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MessageType)
                .ToListAsync();
            return View(leadershipMessages);
        }
        
        public async Task<IActionResult> MessageOfSecretary()
        {
            var leadershipMessages = await _context.LeadershipMessages
                .Where(m => m.IsActive && m.MessageType == "Secretary")
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MessageType)
                .ToListAsync();
            return View(leadershipMessages);
        }

        public IActionResult HowDoWeWork()
        {
            return View();
        }

        public IActionResult memberDirectory()
        {
            return View();
        }

        public async Task<IActionResult> ElectedCandidates()
        {
            var electedCandidates = await _context.ElectedCandidates
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.ElectionYear)
                .ToListAsync();
            return View(electedCandidates);
        }
        
        public async Task<IActionResult> PreviousElectedCandidate()
        {
            var previousCandidates = await _context.PreviousCandidates
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.TermPeriod)
                .ToListAsync();
            return View(previousCandidates);
        }

        public IActionResult SubComitys()
        {
            return View();
        }
        public IActionResult CONTRTRIBUTORS()
        {
            return View();
        }


        public IActionResult donate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home");
            }
            return LocalRedirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
      
    }
}
