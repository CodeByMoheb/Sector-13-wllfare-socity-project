using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private static readonly List<string> StaticRoles = new List<string> {
            "অফিস ম্যানেজার",
            "কম্পিউটার অপারেটর",
            "অফিস সহকারী",
            "মাঠ সুপারভাইজার",
            "কমান্ডার",
            "সহঃ কমান্ডার",
            "গার্ড",
            "কালেক্টর",
            "মালি",
            "পিয়ন"
        };
        private void PopulateRoleList(string selected = null)
        {
            var roles = StaticRoles;
            ViewBag.RoleList = roles.Select(r => new SelectListItem { Text = r, Value = r, Selected = (selected != null && selected == r) }).ToList();
        }

        // GET: /Employee
        public IActionResult Index()
        {
            var employees = _context.Employees
                .Include(e => e.Shift)
                .OrderBy(e => e.EmployeeId)
                .ToList();
            return View(employees);
        }

        // GET: /Employee/Create
        public IActionResult Create()
        {
            PopulateRoleList();
            return View();
        }

        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Generate EmployeeId automatically
                var lastEmployee = await _context.Employees
                    .OrderByDescending(e => e.EmployeeId)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastEmployee != null && !string.IsNullOrEmpty(lastEmployee.EmployeeId))
                {
                    if (int.TryParse(lastEmployee.EmployeeId.Replace("EMP", ""), out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                employee.EmployeeId = $"EMP{nextNumber:D4}";

                // Set default category based on role
                employee.Category = employee.Role switch
                {
                    "অফিস ম্যানেজার" or "কম্পিউটার অপারেটর" or "অফিস সহকারী" => "Office Staff",
                    "মাঠ সুপারভাইজার" or "কমান্ডার" or "সহঃ কমান্ডার" or "গার্ড" => "Field Staff",
                    "কালেক্টর" or "মালি" or "পিয়ন" => "Support Staff",
                    _ => "General"
                };

                // Set default password
                var defaultPassword = "123456";
                var (hash, salt) = HashPassword(defaultPassword);
                employee.PasswordHash = hash;
                employee.PasswordSalt = salt;

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                TempData["EmployeeCreated"] = $"Employee created successfully! Employee ID: {employee.EmployeeId}, Default Password: {defaultPassword}";
                return RedirectToAction("Index");
            }
            PopulateRoleList(employee.Role);
            return View(employee);
        }

        // GET: /Employee/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            PopulateRoleList(employee.Role);
            return View(employee);
        }

        // POST: /Employee/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEmployee = await _context.Employees.FindAsync(id);
                    if (existingEmployee == null)
                    {
                        return NotFound();
                    }

                    // Update properties but preserve EmployeeId and password
                    existingEmployee.Name = employee.Name;
                    existingEmployee.Role = employee.Role;
                    existingEmployee.BaseSalary = employee.BaseSalary;
                    existingEmployee.JoiningDate = employee.JoiningDate;
                    existingEmployee.Email = employee.Email;
                    existingEmployee.Phone = employee.Phone;
                    existingEmployee.Address = employee.Address;
                    existingEmployee.ShiftId = employee.ShiftId;
                    existingEmployee.IsActive = employee.IsActive;
                    existingEmployee.UpdatedAt = DateTime.UtcNow;

                    // Update category based on role
                    existingEmployee.Category = employee.Role switch
                    {
                        "অফিস ম্যানেজার" or "কম্পিউটার অপারেটর" or "অফিস সহকারী" => "Office Staff",
                        "মাঠ সুপারভাইজার" or "কমান্ডার" or "সহঃ কমান্ডার" or "গার্ড" => "Field Staff",
                        "কালেক্টর" or "মালি" or "পিয়ন" => "Support Staff",
                        _ => "General"
                    };

                    _context.Update(existingEmployee);
                    await _context.SaveChangesAsync();
                    TempData["EmployeeUpdated"] = "Employee updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            PopulateRoleList(employee.Role);
            return View(employee);
        }

        // GET: /Employee/ResetPassword/{id}
        public async Task<IActionResult> ResetPassword(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: /Employee/ResetPassword/{id}
        [HttpPost, ActionName("ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Reset to default password
            var defaultPassword = "123456";
            var (hash, salt) = HashPassword(defaultPassword);
            employee.PasswordHash = hash;
            employee.PasswordSalt = salt;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["PasswordReset"] = $"Password reset successfully! Employee ID: {employee.EmployeeId}, New Password: {defaultPassword}";
            return RedirectToAction("Index");
        }

        // GET: /Employee/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Shift)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // GET: /Employee/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: /Employee/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["EmployeeDeleted"] = "Employee deleted successfully!";
            }
            return RedirectToAction("Index");
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        // Password hashing methods
        private (string hash, string salt) HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var salt = Guid.NewGuid().ToString();
                var combined = password + salt;
                var bytes = Encoding.UTF8.GetBytes(combined);
                var hash = sha256.ComputeHash(bytes);
                return (Convert.ToBase64String(hash), salt);
            }
        }
    }
} 