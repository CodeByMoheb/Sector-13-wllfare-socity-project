using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class SetupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public SetupController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
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

        public async Task<IActionResult> SeedAttendanceSystem()
        {
            try
            {
                var results = new List<string>();

                // 1. Create default shifts
                if (!await _context.Shifts.AnyAsync())
                {
                    var shifts = new[]
                    {
                        new Shift { Name = "Morning Shift", StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(14, 0, 0), Description = "6 AM to 2 PM (8 hours)", IsActive = true },
                        new Shift { Name = "Afternoon Shift", StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(22, 0, 0), Description = "2 PM to 10 PM (8 hours)", IsActive = true },
                        new Shift { Name = "Night Shift", StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(6, 0, 0), Description = "10 PM to 6 AM (8 hours)", IsActive = true }
                    };

                    _context.Shifts.AddRange(shifts);
                    await _context.SaveChangesAsync();
                    results.Add($"✅ Created {shifts.Length} default shifts");
                }
                else
                {
                    results.Add("⚠️ Shifts already exist");
                }

                // 2. Update existing employees with EmployeeId and Category
                var employees = await _context.Employees.ToListAsync();
                var counter = 1;

                foreach (var employee in employees)
                {
                    if (string.IsNullOrEmpty(employee.EmployeeId))
                    {
                        employee.EmployeeId = $"EMP{counter:D4}";
                        counter++;
                    }

                    if (string.IsNullOrEmpty(employee.Category))
                    {
                        employee.Category = employee.Role switch
                        {
                            "অফিস ম্যানেজার" or "কম্পিউটার অপারেটর" or "অফিস সহকারী" => "Office Staff",
                            "মাঠ সুপারভাইজার" or "কমান্ডার" or "সহঃ কমান্ডার" or "গার্ড" => "Field Staff",
                            "কালেক্টর" or "মালি" or "পিয়ন" => "Support Staff",
                            _ => "General"
                        };
                    }

                    // Assign default shift based on role
                    if (!employee.ShiftId.HasValue)
                    {
                        var defaultShift = employee.Role switch
                        {
                            "অফিস ম্যানেজার" or "কম্পিউটার অপারেটর" or "অফিস সহকারী" => await _context.Shifts.FirstOrDefaultAsync(s => s.Name == "Morning Shift"),
                            "মাঠ সুপারভাইজার" or "কমান্ডার" or "সহঃ কমান্ডার" => await _context.Shifts.FirstOrDefaultAsync(s => s.Name == "Afternoon Shift"),
                            "গার্ড" => await _context.Shifts.FirstOrDefaultAsync(s => s.Name == "Night Shift"),
                            "কালেক্টর" or "মালি" or "পিয়ন" => await _context.Shifts.FirstOrDefaultAsync(s => s.Name == "Morning Shift"),
                            _ => await _context.Shifts.FirstOrDefaultAsync(s => s.Name == "Morning Shift")
                        };

                        if (defaultShift != null)
                        {
                            employee.ShiftId = defaultShift.ShiftId;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                results.Add($"✅ Updated {employees.Count} employees with EmployeeId and Category");

                // 3. Create sample attendance records for today (if none exist)
                var today = DateTime.Today;
                var todayAttendances = await _context.Attendances
                    .Where(a => a.Date == today)
                    .ToListAsync();

                if (!todayAttendances.Any())
                {
                    var sampleAttendances = new List<Attendance>();
                    var random = new Random();

                    foreach (var employee in employees.Take(5)) // Create sample for first 5 employees
                    {
                        var checkInTime = today.AddHours(8).AddMinutes(random.Next(0, 30)); // Between 8:00-8:30 AM
                        var checkOutTime = today.AddHours(16).AddMinutes(random.Next(0, 60)); // Between 4:00-5:00 PM

                        sampleAttendances.Add(new Attendance
                        {
                            EmployeeId = employee.Id,
                            Date = today,
                            CheckInTime = checkInTime,
                            CheckOutTime = checkOutTime,
                            Status = checkInTime.TimeOfDay <= new TimeSpan(8, 15, 0) ? "On-time" : "Late",
                            TotalHours = 8.0m,
                            Location = "Office",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    _context.Attendances.AddRange(sampleAttendances);
                    await _context.SaveChangesAsync();
                    results.Add($"✅ Created {sampleAttendances.Count} sample attendance records for today");
                }

                ViewBag.Results = results;
                ViewBag.Success = true;
                return View("CreateTestUsers"); // Reuse the same view
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error seeding attendance system: {ex.Message}";
                return View("CreateTestUsers");
            }
        }
    }
} 