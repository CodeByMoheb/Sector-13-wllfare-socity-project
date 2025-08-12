using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Admin,President,Secretary,Manager")]
    public class PermanentMemberController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PermanentMemberController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PermanentMember/ManagerList
        [Authorize(Roles = "Manager,Admin,President,Secretary")]
        public async Task<IActionResult> ManagerList()
        {
            var members = await _context.PermanentMembers.AsNoTracking()
                .AsNoTracking()
                .OrderBy(m => m.Name)
                .ToListAsync();
            return View(members);
        }

        // GET: PermanentMember
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;

            var members = from m in _context.PermanentMembers.AsNoTracking()
                         select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                members = members.Where(m => m.Name.Contains(searchString) 
                                         || m.PhoneNumber.Contains(searchString)
                                         || m.Sector.Contains(searchString)
                                         || m.RoadNo.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    members = members.OrderByDescending(m => m.Name);
                    break;
                case "Date":
                    members = members.OrderBy(m => m.MembershipDate);
                    break;
                case "date_desc":
                    members = members.OrderByDescending(m => m.MembershipDate);
                    break;
                default:
                    members = members.OrderBy(m => m.Name);
                    break;
            }

            int pageSize = 20;
            return View(await PaginatedList<PermanentMember>.CreateAsync(members.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: PermanentMember/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PermanentMember/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,FathersOrHusbandsName,Address,RoadNo,HouseNo,Sector,PhoneNumber,Email,DateOfBirth,NationalId,MembershipDate,Notes")] PermanentMember permanentMember)
        {
            if (ModelState.IsValid)
            {
                permanentMember.CreatedAt = DateTime.Now;
                permanentMember.IsActive = true;
                
                _context.Add(permanentMember);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Permanent member added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(permanentMember);
        }

        // GET: PermanentMember/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permanentMember = await _context.PermanentMembers.FindAsync(id);
            if (permanentMember == null)
            {
                return NotFound();
            }
            return View(permanentMember);
        }

        // POST: PermanentMember/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,FathersOrHusbandsName,Address,RoadNo,HouseNo,Sector,PhoneNumber,Email,DateOfBirth,NationalId,MembershipDate,IsActive,Notes,CreatedAt")] PermanentMember permanentMember)
        {
            if (id != permanentMember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    permanentMember.UpdatedAt = DateTime.Now;
                    _context.Update(permanentMember);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Member updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PermanentMemberExists(permanentMember.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(permanentMember);
        }

        // GET: PermanentMember/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permanentMember = await _context.PermanentMembers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (permanentMember == null)
            {
                return NotFound();
            }

            return View(permanentMember);
        }

        // GET: PermanentMember/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var permanentMember = await _context.PermanentMembers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (permanentMember == null)
            {
                return NotFound();
            }

            return View(permanentMember);
        }

        // POST: PermanentMember/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var permanentMember = await _context.PermanentMembers.FindAsync(id);
            if (permanentMember != null)
            {
                _context.PermanentMembers.Remove(permanentMember);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: PermanentMember/Import
        public IActionResult Import()
        {
            return View();
        }

        // POST: PermanentMember/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Import));
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Please upload a CSV file.";
                return RedirectToAction(nameof(Import));
            }

            try
            {
                var members = new List<PermanentMember>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    // Skip header row
                    await reader.ReadLineAsync();
                    
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 7)
                        {
                            var member = new PermanentMember
                            {
                                Name = parts[0].Trim(),
                                FathersOrHusbandsName = parts[1].Trim(),
                                Address = parts[2].Trim(),
                                RoadNo = parts[3].Trim(),
                                HouseNo = parts[4].Trim(),
                                Sector = parts[5].Trim(),
                                PhoneNumber = parts[6].Trim(),
                                Email = parts.Length > 7 ? parts[7].Trim() : null,
                                NationalId = parts.Length > 8 ? parts[8].Trim() : null,
                                MembershipDate = DateTime.Now,
                                CreatedAt = DateTime.Now,
                                IsActive = true
                            };
                            
                            members.Add(member);
                        }
                    }
                }

                if (members.Any())
                {
                    _context.PermanentMembers.AddRange(members);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Successfully imported {members.Count} permanent members.";
                }
                else
                {
                    TempData["Error"] = "No valid members found in the file.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error importing file: {ex.Message}";
            }

            return RedirectToAction(nameof(Import));
        }

        private bool PermanentMemberExists(int id)
        {
            return _context.PermanentMembers.Any(e => e.Id == id);
        }
    }


}
