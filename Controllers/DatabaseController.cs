using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Microsoft.AspNetCore.Authorization;

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

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedEmployeesToIdentity()
        {
            var messages = new List<string>();

            // Ensure Member role exists
            if (!await _roleManager.RoleExistsAsync("Member"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Member"));
            }

            var employees = await _context.Employees.ToListAsync();
            int created = 0, updated = 0, skipped = 0;

            foreach (var emp in employees)
            {
                var userName = emp.EmployeeId;
                if (string.IsNullOrWhiteSpace(userName)) { skipped++; continue; }

                var user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    var email = !string.IsNullOrWhiteSpace(emp.Email) ? emp.Email : $"{userName}@wfs-13.org";
                    var newUser = new ApplicationUser
                    {
                        UserName = userName,
                        Email = email,
                        EmailConfirmed = true,
                        PhoneNumber = string.IsNullOrWhiteSpace(emp.Phone) ? "0000000000" : emp.Phone,
                        PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(emp.Phone),
                        Name = emp.Name
                    };
                    // Must satisfy Identity password policy: upper, lower, digit, non-alphanumeric
                    var result = await _userManager.CreateAsync(newUser, "Emp@1234");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(newUser, "Member");
                        created++;
                    }
                    else
                    {
                        messages.Add($"❌ {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    // Optionally update profile fields
                    bool dirty = false;
                    if (string.IsNullOrWhiteSpace(user.Name) && !string.IsNullOrWhiteSpace(emp.Name)) { user.Name = emp.Name; dirty = true; }
                    if (string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(emp.Email)) { user.Email = emp.Email; user.EmailConfirmed = true; dirty = true; }
                    if (dirty) { await _userManager.UpdateAsync(user); updated++; }
                }
            }

            TempData["SeedSummary"] = $"Employees synced to Identity. Created: {created}, Updated: {updated}, Skipped: {skipped}. Default password: Emp@1234. Use EmployeeID as username at /Account/Login.";
            return RedirectToAction("Index", "Employee");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetEmployeePasswords()
        {
            var users = await _userManager.Users
                .Where(u => u.UserName != null && u.UserName.StartsWith("EMP"))
                .ToListAsync();

            int reset = 0, failed = 0;
            var messages = new List<string>();

            foreach (var user in users)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var res = await _userManager.ResetPasswordAsync(user, token, "Emp@1234");
                if (res.Succeeded)
                {
                    reset++;
                }
                else
                {
                    failed++;
                    messages.Add($"❌ {user.UserName}: {string.Join(", ", res.Errors.Select(e => e.Description))}");
                }
            }

            TempData["SeedSummary"] = $"Employee passwords reset. Reset: {reset}, Failed: {failed}. New default: Emp@1234";
            if (messages.Any()) TempData["SeedDetails"] = string.Join("; ", messages);
            return RedirectToAction("Index", "Employee");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TestSyncEmployees()
        {
            try
            {
                var messages = new List<string>();

                // Ensure Member role exists
                if (!await _roleManager.RoleExistsAsync("Member"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Member"));
                    messages.Add("✅ Created Member role");
                }

                var employees = await _context.Employees.ToListAsync();
                int created = 0, updated = 0, skipped = 0;

                foreach (var emp in employees)
                {
                    var userName = emp.EmployeeId;
                    if (string.IsNullOrWhiteSpace(userName)) { skipped++; continue; }

                    var user = await _userManager.FindByNameAsync(userName);
                    if (user == null)
                    {
                        var email = !string.IsNullOrWhiteSpace(emp.Email) ? emp.Email : $"{userName}@wfs-13.org";
                        var newUser = new ApplicationUser
                        {
                            UserName = userName,
                            Email = email,
                            EmailConfirmed = true,
                            PhoneNumber = string.IsNullOrWhiteSpace(emp.Phone) ? "0000000000" : emp.Phone,
                            PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(emp.Phone),
                            Name = emp.Name
                        };
                        var result = await _userManager.CreateAsync(newUser, "Emp@1234");
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(newUser, "Member");
                            created++;
                            messages.Add($"✅ Created user: {userName}");
                        }
                        else
                        {
                            messages.Add($"❌ {userName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        updated++;
                        messages.Add($"ℹ️ User already exists: {userName}");
                    }
                }

                ViewBag.Messages = messages;
                ViewBag.Summary = $"Sync completed. Created: {created}, Updated: {updated}, Skipped: {skipped}";
                ViewBag.Success = true;
                return View("SyncResult");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error during sync: {ex.Message}";
                ViewBag.Success = false;
                return View("SyncResult");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TestResetEmployeePasswords()
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => u.UserName != null && u.UserName.StartsWith("EMP"))
                    .ToListAsync();

                int reset = 0, failed = 0;
                var messages = new List<string>();

                foreach (var user in users)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var res = await _userManager.ResetPasswordAsync(user, token, "Emp@1234");
                    if (res.Succeeded)
                    {
                        reset++;
                        messages.Add($"✅ Reset password: {user.UserName}");
                    }
                    else
                    {
                        failed++;
                        messages.Add($"❌ {user.UserName}: {string.Join(", ", res.Errors.Select(e => e.Description))}");
                    }
                }

                ViewBag.Messages = messages;
                ViewBag.Summary = $"Password reset completed. Reset: {reset}, Failed: {failed}.";
                ViewBag.Success = failed == 0;
                return View("SyncResult");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error during reset: {ex.Message}";
                ViewBag.Success = false;
                return View("SyncResult");
            }
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