using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public DatabaseController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> ResetDatabase()
        {
            try
            {
                // Clear all existing users
                var allUsers = await _userManager.Users.ToListAsync();
                foreach (var user in allUsers)
                {
                    await _userManager.DeleteAsync(user);
                }

                // Clear all existing roles
                var allRoles = await _roleManager.Roles.ToListAsync();
                foreach (var role in allRoles)
                {
                    await _roleManager.DeleteAsync(role);
                }

                // Create roles
                var roles = new[] { "Admin", "President", "Secretary", "Manager", "Member" };
                foreach (var role in roles)
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                // Create test users for each role
                var testUsers = new[]
                {
                    new { Email = "admin@sec13.com", UserName = "admin@sec13.com", Role = "Admin", Password = "Admin@123" },
                    new { Email = "president@sec13.com", UserName = "president@sec13.com", Role = "President", Password = "President@123" },
                    new { Email = "secretary@sec13.com", UserName = "secretary@sec13.com", Role = "Secretary", Password = "Secretary@123" },
                    new { Email = "manager@sec13.com", UserName = "manager@sec13.com", Role = "Manager", Password = "Manager@123" },
                    new { Email = "member@sec13.com", UserName = "member@sec13.com", Role = "Member", Password = "Member@123" }
                };

                var results = new List<string>();

                foreach (var userInfo in testUsers)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userInfo.UserName,
                        Email = userInfo.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, userInfo.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, userInfo.Role);
                        results.Add($"✅ Created {userInfo.Role}: {userInfo.Email} (Password: {userInfo.Password})");
                    }
                    else
                    {
                        results.Add($"❌ Failed to create {userInfo.Role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                ViewBag.Results = results;
                ViewBag.Success = true;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error resetting database: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> ViewUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new List<UserRoleInfo>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleInfo
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return View(userRoles);
        }
    }

    public class UserRoleInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
} 