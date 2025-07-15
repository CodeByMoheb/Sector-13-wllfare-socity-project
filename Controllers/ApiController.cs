using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Collections.Generic;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        [HttpGet("home")]
        public IActionResult GetHomeData()
        {
            var homeData = new
            {
                title = "Sector 13 Welfare Society",
                description = "Welcome to Sector 13 Welfare Society - Digital Management System",
                features = new[]
                {
                    new { title = "Community Service", description = "Serving the community with dedication" },
                    new { title = "Digital Management", description = "Modern digital solutions for better management" },
                    new { title = "Transparency", description = "Complete transparency in all operations" }
                }
            };
            
            return Ok(homeData);
        }

        [HttpGet("about-us")]
        public IActionResult GetAboutUsData()
        {
            var aboutData = new
            {
                title = "About Sector 13 Welfare Society",
                description = "We are dedicated to serving the community of Sector 13 with integrity and commitment.",
                mission = "To improve the quality of life for all residents of Sector 13",
                vision = "A prosperous and harmonious community"
            };
            
            return Ok(aboutData);
        }

        [HttpGet("sub-committees")]
        public IActionResult GetSubCommittees()
        {
            var committees = new[]
            {
                new {
                    id = 1,
                    name = "A_© Dc-KwgwU",
                    members = new[]
                    {
                        new { name = "BwÄwbqvi KvRx mwdKzj Bmjvg", phone = "01720923328", role = "AvnŸvqK" },
                        new { name = "‡gvt jyrdi ingvb ZvjyK`vi", phone = "01713001353", role = "m`m¨" },
                        new { name = "Aa¨vcK †kL gvngy` Avjg", phone = "01321128080", role = "m`m¨" },
                        new { name = "Gm Gg mvBdz¾vgvb gvwbK", phone = "01911888603", role = "m`m¨" },
                        new { name = "‡gvt Av‡bvqvi †nv‡mb", phone = "01619127842", role = "m`m¨" }
                    }
                },
                new {
                    id = 2,
                    name = "wbivcËv Ges AvevwmK GjvKvq",
                    members = new[]
                    {
                        new { name = "‡gvt jyrdi ingvb ZvjyK`vi", phone = "01713001353", role = "AvnŸvqK" },
                        new { name = "Aa¨vcK †kL gvngy` Avjg", phone = "01321128080", role = "m`m¨" },
                        new { name = "‡gvnv¤§` `yjvj DwÏb", phone = "01711603446", role = "m`m¨" },
                        new { name = "‡gvt Avhnviæj Bmjvg Ry‡qj", phone = "01819231856", role = "m`m¨" },
                        new { name = "BwÄwbqvi †gvt Rvjvj DwÏb Lvb", phone = "01712115062", role = "m`m¨" },
                        new { name = "Gm Gg mvBdz¾vgvb gvwbK", phone = "01911888603", role = "m`m¨" },
                        new { name = "Av‡bvqvi †nv‡mb", phone = "01619127842", role = "m`m¨" }
                    }
                },
                new {
                    id = 3,
                    name = "µxov I we‡bv`b Ges †Ljvi gvV",
                    members = new[]
                    {
                        new { name = "‡gvnv¤§` `yjvj DwÏb", phone = "01711603446", role = "AvnŸvqK" },
                        new { name = "‡gvt jyrdi ingvb ZvjyK`vi", phone = "01713001353", role = "m`m¨" },
                        new { name = "Aa¨vcK †kL gvngy` Avjg", phone = "01321128080", role = "m`m¨" },
                        new { name = "‡gvt Avhnviæj Bmjvg Ry‡qj", phone = "01819231856", role = "m`m¨" },
                        new { name = "Gm Gg mvBdz¾vgvb gvwbK", phone = "01911888603", role = "m`m¨" },
                        new { name = "BwÄwbqvi †gvt Rvjvj DwÏb Lvb", phone = "01712115062", role = "m`m¨" },
                        new { name = "wiqvRyj Avjg", phone = "01712781177", role = "m`m¨" },
                        new { name = "Avd‡ivRv †eMg", phone = "01927118489", role = "m`m¨" }
                    }
                }
            };
            
            return Ok(committees);
        }

        [HttpGet("elected-candidates")]
        public IActionResult GetElectedCandidates()
        {
            var candidates = new[]
            {
                new { name = "Candidate 1", position = "President", phone = "01712345678" },
                new { name = "Candidate 2", position = "Secretary", phone = "01787654321" },
                new { name = "Candidate 3", position = "Treasurer", phone = "01711223344" }
            };
            
            return Ok(candidates);
        }

        [HttpGet("previous-elected-candidates")]
        public IActionResult GetPreviousElectedCandidates()
        {
            var candidates = new[]
            {
                new { name = "Previous Candidate 1", position = "President", year = "2023" },
                new { name = "Previous Candidate 2", position = "Secretary", year = "2023" },
                new { name = "Previous Candidate 3", position = "Treasurer", year = "2023" }
            };
            
            return Ok(candidates);
        }

        [HttpGet("gallery")]
        public IActionResult GetGalleryImages()
        {
            var images = new[]
            {
                new { id = 1, url = "/Photos/logo.png", title = "Society Logo", description = "Official logo of Sector 13 Welfare Society" },
                new { id = 2, url = "/Photos/logo.png", title = "Community Event", description = "Recent community gathering" },
                new { id = 3, url = "/Photos/logo.png", title = "Meeting", description = "Committee meeting" }
            };
            
            return Ok(images);
        }

        [HttpGet("message-of-chairman")]
        public IActionResult GetMessageOfChairman()
        {
            var message = new
            {
                title = "Message from the Chairman",
                content = "Welcome to Sector 13 Welfare Society. We are committed to serving our community with dedication and transparency.",
                author = "Chairman",
                date = "2025"
            };
            
            return Ok(message);
        }

        [HttpGet("how-do-we-work")]
        public IActionResult GetHowDoWeWorkData()
        {
            var data = new
            {
                title = "How Do We Work",
                description = "Our working methodology and processes",
                steps = new[]
                {
                    new { step = 1, title = "Planning", description = "Strategic planning for community development" },
                    new { step = 2, title = "Implementation", description = "Executing planned activities" },
                    new { step = 3, title = "Monitoring", description = "Regular monitoring and evaluation" },
                    new { step = 4, title = "Feedback", description = "Collecting community feedback" }
                }
            };
            
            return Ok(data);
        }

        [HttpGet("member-directory")]
        public IActionResult GetMemberDirectory()
        {
            var members = new[]
            {
                new { id = 1, name = "Member 1", phone = "01712345678", email = "member1@example.com" },
                new { id = 2, name = "Member 2", phone = "01787654321", email = "member2@example.com" },
                new { id = 3, name = "Member 3", phone = "01711223344", email = "member3@example.com" }
            };
            
            return Ok(members);
        }

        [HttpPost("contact")]
        public IActionResult SubmitContact([FromBody] ContactFormModel model)
        {
            // Here you would typically save to database and send email
            return Ok(new { success = true, message = "Contact form submitted successfully" });
        }
    }

    public class ContactFormModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
} 