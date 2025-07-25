using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            "পিয়ন"
        };
        private void PopulateRoleList(string selected = null)
        {
            var roles = StaticRoles;
            ViewBag.RoleList = roles.Select(r => new SelectListItem { Text = r, Value = r, Selected = (selected != null && selected == r) }).ToList();
        }

        // GET: /Employee
        public IActionResult Index()
        {
            var employees = _context.Employees.ToList();
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
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateRoleList(employee.Role);
            return View(employee);
        }

        // GET: /Employee/Edit/5
        public IActionResult Edit(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null) return NotFound();
            PopulateRoleList(employee.Role);
            return View(employee);
        }

        // POST: /Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateRoleList(employee.Role);
            return View(employee);
        }

        // GET: /Employee/Delete/5
        public IActionResult Delete(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        // POST: /Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
} 