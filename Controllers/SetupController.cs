using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class SetupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SetupController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> CreateTestUsers()
        {
            try
            {
                // Ensure roles exist
                var roles = new[] { "Admin", "President", "Secretary", "Manager", "Member" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Create test users for each role
                var users = new[]
                {
                    new { Email = "admin@sec13.com", UserName = "admin", Role = "Admin" },
                    new { Email = "president@sec13.com", UserName = "president", Role = "President" },
                    new { Email = "secretary@sec13.com", UserName = "secretary", Role = "Secretary" },
                    new { Email = "manager@sec13.com", UserName = "manager", Role = "Manager" },
                    new { Email = "member@sec13.com", UserName = "member", Role = "Member" }
                };

                var results = new List<string>();

                foreach (var userInfo in users)
                {
                    var existingUser = await _userManager.FindByEmailAsync(userInfo.Email);
                    if (existingUser == null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = userInfo.UserName,
                            Email = userInfo.Email,
                            EmailConfirmed = true
                        };

                        var result = await _userManager.CreateAsync(user, "Test@123");
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, userInfo.Role);
                            results.Add($"✅ Created {userInfo.Role}: {userInfo.Email} (Password: Test@123)");
                        }
                        else
                        {
                            results.Add($"❌ Failed to create {userInfo.Role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        results.Add($"⚠️ User {userInfo.Email} already exists");
                    }
                }

                ViewBag.Results = results;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error creating users: {ex.Message}";
                return View();
            }
        }
    }
} 