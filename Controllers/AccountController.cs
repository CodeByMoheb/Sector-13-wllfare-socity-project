using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Security.Claims;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Try to find user by email first, then by username
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(model.Email);
                }

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        
                        // Redirect to role-specific dashboard
                        if (roles.Contains("Admin"))
                            return RedirectToAction("Admin", "Dashboard");
                        else if (roles.Contains("President"))
                            return RedirectToAction("President", "Dashboard");
                        else if (roles.Contains("Secretary"))
                            return RedirectToAction("Secretary", "Dashboard");
                        else if (roles.Contains("Manager"))
                            return RedirectToAction("Manager", "Dashboard");
                        else if (roles.Contains("Member"))
                            return RedirectToAction("Member", "Dashboard");
                        else
                            return RedirectToAction("Index", "Dashboard");
                    }
                }
                
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.Roles = GetAvailableRoles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.Roles = GetAvailableRoles();

            if (ModelState.IsValid)
            {
                // Only allow 'Member' registration from public form
                model.SelectedRole = "Member";
                var user = new IdentityUser { UserName = model.Email, Email = model.Email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Assign role
                    if (!string.IsNullOrEmpty(model.SelectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    }

                    // Sign in the user
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Redirect to role-specific dashboard
                    if (model.SelectedRole == "Admin")
                        return RedirectToAction("Admin", "Dashboard");
                    else if (model.SelectedRole == "President")
                        return RedirectToAction("President", "Dashboard");
                    else if (model.SelectedRole == "Secretary")
                        return RedirectToAction("Secretary", "Dashboard");
                    else if (model.SelectedRole == "Manager")
                        return RedirectToAction("Manager", "Dashboard");
                    else if (model.SelectedRole == "Member")
                        return RedirectToAction("Member", "Dashboard");
                    else
                        return RedirectToAction("Index", "Dashboard");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user (removes authentication cookie)
            await _signInManager.SignOutAsync();

            HttpContext.Session.Clear();
            // Redirect to Home page
            return RedirectToAction("Index", "Home");
        }

        private List<string> GetAvailableRoles()
        {
            return new List<string> { "Member", "Manager", "Secretary", "President", "Admin" };
        }

        // Add this method to seed a SuperAdmin if not present
        private async Task EnsureSuperAdminExists()
        {
            var superAdminEmail = "superadmin@sec13.com";
            var superAdminUser = await _userManager.FindByEmailAsync(superAdminEmail);
            if (superAdminUser == null)
            {
                var user = new IdentityUser { UserName = superAdminEmail, Email = superAdminEmail, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, "SuperAdmin@123");
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
                        await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
                    await _userManager.AddToRoleAsync(user, "SuperAdmin");
                }
            }
        }
    }
} 
