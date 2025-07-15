using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
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
                        user.LastLoginTime = DateTime.Now;
                        await _userManager.UpdateAsync(user);
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
                var user = new ApplicationUser { 
                    UserName = model.Email, 
                    Email = model.Email, 
                    EmailConfirmed = true,
                    PhoneNumber = model.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    Name = $"{model.FirstName} {model.LastName}".Trim()
                };
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


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var model = new EditProfileViewModel
            {
                Name = user.Name,
                FathersOrHusbandsName = user.FathersOrHusbandsName,
                HouseNo = user.HouseNo,
                Ward = user.Ward,
                Holding = user.Holding,
                Sector = user.Sector,
                Profession = user.Profession,
                Designation = user.Designation,
                BloodGroup = user.BloodGroup,
                EducationalQualification = user.EducationalQualification,
                NumberOfChildren = user.NumberOfChildren,
                Telephone = user.Telephone,
                Mobile = user.PhoneNumber,
                Email = user.Email,
                ExistingProfilePictureUrl = user.ProfilePictureUrl
            };
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var model = new EditProfileViewModel
            {
                Name = user.Name,
                FathersOrHusbandsName = user.FathersOrHusbandsName,
                HouseNo = user.HouseNo,
                Ward = user.Ward,
                Holding = user.Holding,
                Sector = user.Sector,
                Profession = user.Profession,
                Designation = user.Designation,
                BloodGroup = user.BloodGroup,
                EducationalQualification = user.EducationalQualification,
                NumberOfChildren = user.NumberOfChildren,
                Telephone = user.Telephone,
                Mobile = user.PhoneNumber,
                Email = user.Email,
                ExistingProfilePictureUrl = user.ProfilePictureUrl
            };
            model.FlatNo = user.FlatNo;
            model.RoadNo = user.RoadNo;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            user.Name = model.Name;
            user.FathersOrHusbandsName = model.FathersOrHusbandsName;
            user.HouseNo = model.HouseNo;
            user.Ward = model.Ward;
            user.Holding = model.Holding;
            user.Sector = model.Sector;
            user.Profession = model.Profession;
            user.Designation = model.Designation;
            user.BloodGroup = model.BloodGroup;
            user.EducationalQualification = model.EducationalQualification;
            user.NumberOfChildren = model.NumberOfChildren;
            user.Telephone = model.Telephone;
            user.PhoneNumber = model.Mobile;
            user.Email = model.Email;
            user.FlatNo = model.FlatNo;
            user.RoadNo = model.RoadNo;
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var fileName = $"profile_{user.Id}_{DateTime.Now.Ticks}{System.IO.Path.GetExtension(model.ProfilePicture.FileName)}";
                var filePath = Path.Combine("wwwroot/Photos/ProfilePhotos", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }
                user.ProfilePictureUrl = $"/Photos/ProfilePhotos/{fileName}";
            }
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Profile");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    // For now, we'll just show a message. In a real application, you would send an email
                    TempData["InfoMessage"] = "If an account with that email exists, a password reset link has been sent.";
                }
                else
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    TempData["InfoMessage"] = "If an account with that email exists, a password reset link has been sent.";
                }
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
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
                var user = new ApplicationUser { UserName = superAdminEmail, Email = superAdminEmail, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, "SuperAdmin@123");
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
                        await _roleManager.CreateAsync(new IdentityRole("SuperAdmin"));
                    await _userManager.AddToRoleAsync(user, "SuperAdmin");
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                // Update any authentication tokens
                await _signInManager.UpdateExternalAuthenticationTokensAsync(info);
                return LocalRedirect(returnUrl);
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email ?? string.Empty });
            }
        }
    }
} 
