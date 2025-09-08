using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace SeedAttendanceData
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await SeedData(host.Services);
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Sector13WelfareSociety;Trusted_Connection=true;MultipleActiveResultSets=true"));
                });

        static async Task SeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                Console.WriteLine("Starting to seed attendance system data...");

                // 1. Create default shifts
                if (!await context.Shifts.AnyAsync())
                {
                    var shifts = new[]
                    {
                        new Shift { Name = "Morning Shift", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(16, 0, 0), Description = "8 AM to 4 PM", IsActive = true },
                        new Shift { Name = "Evening Shift", StartTime = new TimeSpan(16, 0, 0), EndTime = new TimeSpan(0, 0, 0), Description = "4 PM to 12 AM", IsActive = true },
                        new Shift { Name = "Night Shift", StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(8, 0, 0), Description = "12 AM to 8 AM", IsActive = true },
                        new Shift { Name = "Day Shift", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), Description = "9 AM to 5 PM", IsActive = true }
                    };

                    context.Shifts.AddRange(shifts);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Created {shifts.Length} default shifts");
                }
                else
                {
                    Console.WriteLine("⚠️ Shifts already exist");
                }

                // 2. Update existing employees with EmployeeId and Category
                var employees = await context.Employees.ToListAsync();
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
                            "অফিস ম্যানেজার" or "কম্পিউটার অপারেটর" or "অফিস সহকারী" => await context.Shifts.FirstOrDefaultAsync(s => s.Name == "Day Shift"),
                            "মাঠ সুপারভাইজার" or "কমান্ডার" or "সহঃ কমান্ডার" or "গার্ড" => await context.Shifts.FirstOrDefaultAsync(s => s.Name == "Morning Shift"),
                            "কালেক্টর" or "মালি" or "পিয়ন" => await context.Shifts.FirstOrDefaultAsync(s => s.Name == "Morning Shift"),
                            _ => await context.Shifts.FirstOrDefaultAsync(s => s.Name == "Day Shift")
                        };

                        if (defaultShift != null)
                        {
                            employee.ShiftId = defaultShift.ShiftId;
                        }
                    }
                }

                await context.SaveChangesAsync();
                Console.WriteLine($"✅ Updated {employees.Count} employees with EmployeeId and Category");

                // 3. Create sample attendance records for today (if none exist)
                var today = DateTime.Today;
                var todayAttendances = await context.Attendances
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

                    context.Attendances.AddRange(sampleAttendances);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ Created {sampleAttendances.Count} sample attendance records for today");
                }

                Console.WriteLine("🎉 Attendance system data seeding completed successfully!");
                Console.WriteLine("\nEmployee Login Credentials:");
                Console.WriteLine("==========================");
                foreach (var employee in employees.Take(5))
                {
                    Console.WriteLine($"Employee ID: {employee.EmployeeId} | Name: {employee.Name} | Role: {employee.Role}");
                }
                Console.WriteLine("\nNote: For now, any password will work for employee login (password validation needs to be implemented).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding data: {ex.Message}");
            }
        }
    }
}
