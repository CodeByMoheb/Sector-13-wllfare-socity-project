using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    // [Authorize(Roles = "Manager,Secretary,SuperAdmin")] // Temporarily disabled for testing
    public class ContentManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContentManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Leadership Messages Management
        public async Task<IActionResult> LeadershipMessages()
        {
            var messages = await _context.LeadershipMessages
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.MessageType)
                .ToListAsync();
            return View(messages);
        }

        [HttpGet]
        public IActionResult CreateLeadershipMessage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLeadershipMessage(LeadershipMessage message, IFormFile imageFile)
        {
            // Debug: Log the incoming model data and form data
            var formData = Request.Form;
            TempData["Debug"] = $"Form Data - Name: '{formData["Name"]}', Designation: '{formData["Designation"]}', MessageType: '{formData["MessageType"]}', Message: '{formData["Message"]}'. Model Data - Name: '{message.Name}', Designation: '{message.Designation}', MessageType: '{message.MessageType}', Message: '{message.Message}'";

            // Try to manually bind form data if model binding failed
            if (string.IsNullOrEmpty(message.Name) && !string.IsNullOrEmpty(formData["Name"]))
            {
                message.Name = formData["Name"];
            }
            if (string.IsNullOrEmpty(message.Designation) && !string.IsNullOrEmpty(formData["Designation"]))
            {
                message.Designation = formData["Designation"];
            }
            if (string.IsNullOrEmpty(message.MessageType) && !string.IsNullOrEmpty(formData["MessageType"]))
            {
                message.MessageType = formData["MessageType"];
            }
            if (string.IsNullOrEmpty(message.Message) && !string.IsNullOrEmpty(formData["Message"]))
            {
                message.Message = formData["Message"];
            }
            if (string.IsNullOrEmpty(message.Phone) && !string.IsNullOrEmpty(formData["Phone"]))
            {
                message.Phone = formData["Phone"];
            }

            // Set the CreatedBy field before validation
            message.CreatedBy = User.Identity?.Name ?? "System";
            message.CreatedAt = DateTime.Now;
            message.IsActive = true;

            // Remove CreatedBy from validation since we're setting it manually
            ModelState.Remove("CreatedBy");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("IsActive");

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos", "Leadership");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    
                    message.ImageUrl = $"/Photos/Leadership/{fileName}";
                }

                try
                {
                    _context.LeadershipMessages.Add(message);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Leadership message created successfully.";
                    return RedirectToAction("LeadershipMessages");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving leadership message: {ex.Message}";
                    return View(message);
                }
            }
            else
            {
                // Log model state for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Validation failed: {string.Join(", ", errors)}";
            }
            return View(message);
        }

        [HttpGet]
        public async Task<IActionResult> EditLeadershipMessage(int id)
        {
            var message = await _context.LeadershipMessages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLeadershipMessage(int id, LeadershipMessage message, IFormFile imageFile)
        {
            if (id != message.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing message from database
                    var existingMessage = await _context.LeadershipMessages.FindAsync(id);
                    if (existingMessage == null)
                    {
                        return NotFound();
                    }

                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos", "Leadership");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        
                        existingMessage.ImageUrl = $"/Photos/Leadership/{fileName}";
                    }

                    // Update the existing message properties
                    existingMessage.Name = message.Name;
                    existingMessage.Designation = message.Designation;
                    existingMessage.Phone = message.Phone;
                    existingMessage.Message = message.Message;
                    existingMessage.MessageType = message.MessageType;
                    existingMessage.DisplayOrder = message.DisplayOrder;
                    existingMessage.IsActive = message.IsActive;
                    existingMessage.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Leadership message updated successfully.";
                    return RedirectToAction("LeadershipMessages");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeadershipMessageExists(message.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating leadership message: {ex.Message}";
                    return View(message);
                }
            }
            return View(message);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLeadershipMessage(int id)
        {
            try
            {
                var message = await _context.LeadershipMessages.FindAsync(id);
                if (message != null)
                {
                    message.IsActive = false;
                    message.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Leadership message deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Leadership message not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting leadership message: {ex.Message}";
            }
            return RedirectToAction("LeadershipMessages");
        }

        // Elected Candidates Management
        public async Task<IActionResult> ElectedCandidates()
        {
            var candidates = await _context.ElectedCandidates
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.ElectionYear)
                .ToListAsync();
            return View(candidates);
        }

        [HttpGet]
        public IActionResult CreateElectedCandidate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateElectedCandidate(ElectedCandidate candidate, IFormFile imageFile)
        {
            // Set the CreatedBy field before validation
            candidate.CreatedBy = User.Identity?.Name ?? "System";
            candidate.CreatedAt = DateTime.Now;
            candidate.IsActive = true;

            // Remove CreatedBy from validation since we're setting it manually
            ModelState.Remove("CreatedBy");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("IsActive");

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos", "ElectedCandidates");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    
                    candidate.ImageUrl = $"/Photos/ElectedCandidates/{fileName}";
                }

                _context.ElectedCandidates.Add(candidate);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Elected candidate created successfully.";
                return RedirectToAction("ElectedCandidates");
            }
            return View(candidate);
        }

        [HttpGet]
        public async Task<IActionResult> EditElectedCandidate(int id)
        {
            var candidate = await _context.ElectedCandidates.FindAsync(id);
            if (candidate == null)
            {
                return NotFound();
            }
            return View(candidate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditElectedCandidate(int id, ElectedCandidate candidate, IFormFile imageFile)
        {
            if (id != candidate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos", "ElectedCandidates");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        
                        candidate.ImageUrl = $"/Photos/ElectedCandidates/{fileName}";
                    }

                    candidate.UpdatedAt = DateTime.Now;
                    _context.Update(candidate);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Elected candidate updated successfully.";
                    return RedirectToAction("ElectedCandidates");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ElectedCandidateExists(candidate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(candidate);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteElectedCandidate(int id)
        {
            var candidate = await _context.ElectedCandidates.FindAsync(id);
            if (candidate != null)
            {
                candidate.IsActive = false;
                candidate.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Elected candidate deleted successfully.";
            }
            return RedirectToAction("ElectedCandidates");
        }

        // Previous Candidates Management
        public async Task<IActionResult> PreviousCandidates()
        {
            var candidates = await _context.PreviousCandidates
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.TermPeriod)
                .ToListAsync();
            return View(candidates);
        }

        [HttpGet]
        public IActionResult CreatePreviousCandidate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePreviousCandidate(PreviousCandidate candidate, IFormFile imageFile)
        {
            // Set the CreatedBy field before validation
            candidate.CreatedBy = User.Identity?.Name ?? "System";
            candidate.CreatedAt = DateTime.Now;
            candidate.IsActive = true;

            // Remove CreatedBy from validation since we're setting it manually
            ModelState.Remove("CreatedBy");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("IsActive");

            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos", "PreviousCandidates");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    
                    candidate.ImageUrl = $"/Photos/PreviousCandidates/{fileName}";
                }

                _context.PreviousCandidates.Add(candidate);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Previous candidate created successfully.";
                return RedirectToAction("PreviousCandidates");
            }
            return View(candidate);
        }

        [HttpGet]
        public async Task<IActionResult> EditPreviousCandidate(int id)
        {
            var candidate = await _context.PreviousCandidates.FindAsync(id);
            if (candidate == null)
            {
                return NotFound();
            }
            return View(candidate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPreviousCandidate(int id, PreviousCandidate candidate, IFormFile imageFile)
        {
            if (id != candidate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Photos", "PreviousCandidates");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        
                        candidate.ImageUrl = $"/Photos/PreviousCandidates/{fileName}";
                    }

                    candidate.UpdatedAt = DateTime.Now;
                    _context.Update(candidate);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Previous candidate updated successfully.";
                    return RedirectToAction("PreviousCandidates");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PreviousCandidateExists(candidate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(candidate);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePreviousCandidate(int id)
        {
            var candidate = await _context.PreviousCandidates.FindAsync(id);
            if (candidate != null)
            {
                candidate.IsActive = false;
                candidate.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Previous candidate deleted successfully.";
            }
            return RedirectToAction("PreviousCandidates");
        }

        // Helper methods
        private bool LeadershipMessageExists(int id)
        {
            return _context.LeadershipMessages.Any(e => e.Id == id);
        }

        private bool ElectedCandidateExists(int id)
        {
            return _context.ElectedCandidates.Any(e => e.Id == id);
        }

        private bool PreviousCandidateExists(int id)
        {
            return _context.PreviousCandidates.Any(e => e.Id == id);
        }
    }
}
