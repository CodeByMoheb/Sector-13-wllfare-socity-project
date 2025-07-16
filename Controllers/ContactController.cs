using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Services;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class ContactController : Controller
    {
        private readonly IEmailSender _emailSender;

        public ContactController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        // GET: /Contact/Contact
        [HttpGet]
        public IActionResult Contact()
        {
            return View(); // এটা Views/Contact/Contact.cshtml লোড করবে
        }

        // POST: /Contact/SendMessage
        [HttpPost]
        public async Task<IActionResult> SendMessage(string FullName, string Email, string Message)
        {
            var subject = $"📬 New Message from Contact Form: {FullName}";
            var body = $@"
                <p><strong>Sender Name:</strong> {FullName}</p>
                <p><strong>Sender Email:</strong> {Email}</p>
                <p><strong>Message:</strong></p>
                <p>{Message}</p>
            ";

            await _emailSender.SendEmailAsync("21103027@iubat.edu", subject, body);

            TempData["SuccessMessage"] = "✅ Your message has been sent successfully!";
            return RedirectToAction("ContactUs","Home");
        }
    }
}
