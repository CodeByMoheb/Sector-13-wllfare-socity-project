using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestUsers()
        {
            try
            {
                // Create test users for each role
                var users = new[]
                {
                    new { Email = "admin@sec13.com", UserName = "admin", Role = "Admin" },
                    new { Email = "president@sec13.com", UserName = "president", Role = "President" },
                    new { Email = "secretary@sec13.com", UserName = "secretary", Role = "Secretary" },
                    new { Email = "manager@sec13.com", UserName = "manager", Role = "Manager" },
                    new { Email = "member@sec13.com", UserName = "member", Role = "Member" }
                };

                foreach (var userInfo in users)
                {
                    var existingUser = await _userManager.FindByEmailAsync(userInfo.Email);
                    if (existingUser == null)
                    {
                        var user = new IdentityUser
                        {
                            UserName = userInfo.UserName,
                            Email = userInfo.Email,
                            EmailConfirmed = true
                        };

                        var result = await _userManager.CreateAsync(user, "Test@123");
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, userInfo.Role);
                        }
                    }
                }

                TempData["Message"] = "Test users created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating users: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string email, string role)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Index");
                }

                var result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    TempData["Message"] = $"Role '{role}' assigned to {email} successfully!";
                }
                else
                {
                    TempData["Error"] = $"Error assigning role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ListUsers()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(userRoles);
        }
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }
} 