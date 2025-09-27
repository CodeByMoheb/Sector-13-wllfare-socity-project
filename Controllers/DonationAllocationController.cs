using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Services;
using System.Security.Claims;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Secretary,Admin")]
    public class DonationAllocationController : Controller
    {
        private readonly IDonationAllocationService _allocationService;
        private readonly IProjectManagementService _projectService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DonationAllocationController> _logger;

        public DonationAllocationController(
            IDonationAllocationService allocationService,
            IProjectManagementService projectService,
            ApplicationDbContext context,
            ILogger<DonationAllocationController> logger)
        {
            _allocationService = allocationService;
            _projectService = projectService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var allocations = await _context.DonationAllocations
                    .Include(da => da.Donor)
                    .Include(da => da.Project)
                    .OrderByDescending(da => da.AllocationDate)
                    .ToListAsync();

                return View(allocations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading donation allocations");
                TempData["Error"] = "An error occurred while loading donation allocations.";
                return View(new List<DonationAllocation>());
            }
        }

        public async Task<IActionResult> Allocate()
        {
            try
            {
                // Get completed donations that haven't been fully allocated
                var availableDonors = await _context.Donors
                    .Where(d => d.PaymentStatus == "Completed")
                    .ToListAsync();

                var donorsWithBalance = new List<object>();
                foreach (var donor in availableDonors)
                {
                    var remainingBalance = await _allocationService.GetDonorRemainingBalanceAsync(donor.Id);
                    if (remainingBalance > 0)
                    {
                        donorsWithBalance.Add(new
                        {
                            Id = donor.Id,
                            Name = donor.Name,
                            Email = donor.Email,
                            TotalDonated = donor.Amount,
                            RemainingBalance = remainingBalance,
                            DisplayText = $"{donor.Name} (Available: BDT {remainingBalance:F2})"
                        });
                    }
                }

                var projects = await _projectService.GetAllProjectsAsync();
                var availableProjects = new List<object>();
                foreach (var project in projects.Where(p => p.Status != "Completed" && p.Status != "Cancelled"))
                {
                    // Refresh project data to get updated allocated amounts
                    await _projectService.UpdateProjectAllocatedAmountAsync(project.Id);
                    var updatedProject = await _projectService.GetProjectByIdAsync(project.Id);
                    
                    availableProjects.Add(new
                    {
                        Id = updatedProject.Id,
                        Name = updatedProject.Name,
                        Category = updatedProject.Category,
                        RequiredAmount = updatedProject.RequiredAmount,
                        AllocatedAmount = updatedProject.AllocatedAmount,
                        RemainingAmount = updatedProject.RemainingAmount
                    });
                }

                ViewBag.AvailableDonors = donorsWithBalance;
                ViewBag.AvailableProjects = availableProjects;

                return View(new DonationAllocationRequest());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allocation form data");
                TempData["Error"] = "An error occurred while loading the allocation form.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Allocate(DonationAllocationRequest request)
        {
            if (!ModelState.IsValid)
            {
                // Reload form data
                await LoadAllocationFormData();
                return View(request);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                var allocation = await _allocationService.AllocateDonationAsync(request, userId);

                TempData["Success"] = $"Successfully allocated BDT {request.Amount:F2} from donor to project.";
                return RedirectToAction("AllocationSuccess", new { id = allocation.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadAllocationFormData();
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating donation");
                ModelState.AddModelError("", "An error occurred while allocating the donation. Please try again.");
                await LoadAllocationFormData();
                return View(request);
            }
        }

        public async Task<IActionResult> AllocationSuccess(int id)
        {
            try
            {
                var allocation = await _allocationService.GetAllocationByIdAsync(id);
                if (allocation == null)
                {
                    TempData["Error"] = "Allocation not found.";
                    return RedirectToAction("Index");
                }

                return View(allocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allocation success for allocation {AllocationId}", id);
                TempData["Error"] = "An error occurred while loading allocation details.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var allocation = await _allocationService.GetAllocationByIdAsync(id);
                if (allocation == null)
                {
                    TempData["Error"] = "Allocation not found.";
                    return RedirectToAction("Index");
                }

                return View(allocation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allocation details for allocation {AllocationId}", id);
                TempData["Error"] = "An error occurred while loading allocation details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? utilizationDetails = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                var success = await _allocationService.UpdateAllocationStatusAsync(id, status, utilizationDetails, userId);

                if (success)
                {
                    TempData["Success"] = $"Allocation status updated to {status}.";
                }
                else
                {
                    TempData["Error"] = "Allocation not found or could not be updated.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating allocation status for allocation {AllocationId}", id);
                TempData["Error"] = "An error occurred while updating allocation status.";
            }

            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> GetDonorRecentAllocations(int donorId)
        {
            try
            {
                var allocations = await _context.DonationAllocations
                    .Include(da => da.Project)
                    .Where(da => da.DonorId == donorId)
                    .OrderByDescending(da => da.AllocationDate)
                    .Take(10)
                    .Select(da => new
                    {
                        id = da.Id,
                        allocationDate = da.AllocationDate,
                        projectName = da.Project != null ? da.Project.Name : "Unknown Project",
                        amount = da.Amount,
                        status = da.Status,
                        purpose = da.Purpose
                    })
                    .ToListAsync();

                return Json(new { success = true, allocations = allocations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent allocations for donor {DonorId}", donorId);
                return Json(new { success = false, message = "Error loading recent allocations" });
            }
        }

        public async Task<IActionResult> DonorContributions(int donorId)
        {
            try
            {
                var contributionTracking = await _allocationService.GetDonorContributionTrackingAsync(donorId);
                if (contributionTracking == null)
                {
                    TempData["Error"] = "Donor not found.";
                    return RedirectToAction("Index");
                }

                return View(contributionTracking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading donor contributions for donor {DonorId}", donorId);
                TempData["Error"] = "An error occurred while loading donor contributions.";
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> PublicContributions()
        {
            try
            {
                var contributions = await _allocationService.GetAllDonorContributionsAsync();
                // Filter to show only public information
                var publicContributions = contributions.Where(c => c.TotalAllocated > 0).ToList();
                return View(publicContributions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public contributions");
                TempData["Error"] = "An error occurred while loading contributions.";
                return View(new List<ContributionTrackingViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetDonorBalance(int donorId)
        {
            try
            {
                var balance = await _allocationService.GetDonorRemainingBalanceAsync(donorId);
                return Json(new { success = true, balance = balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting donor balance for donor {DonorId}", donorId);
                return Json(new { success = false, message = "Error retrieving donor balance." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetProjectRemainingAmount(int projectId)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(projectId);
                if (project == null)
                {
                    return Json(new { success = false, message = "Project not found." });
                }

                return Json(new { success = true, remainingAmount = project.RemainingAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project remaining amount for project {ProjectId}", projectId);
                return Json(new { success = false, message = "Error retrieving project information." });
            }
        }

        private async Task LoadAllocationFormData()
        {
            try
            {
                // Get completed donations that haven't been fully allocated
                var availableDonors = await _context.Donors
                    .Where(d => d.PaymentStatus == "Completed")
                    .ToListAsync();

                var donorsWithBalance = new List<object>();
                foreach (var donor in availableDonors)
                {
                    var remainingBalance = await _allocationService.GetDonorRemainingBalanceAsync(donor.Id);
                    if (remainingBalance > 0)
                    {
                        donorsWithBalance.Add(new
                        {
                            Id = donor.Id,
                            Name = donor.Name,
                            Email = donor.Email,
                            TotalDonated = donor.Amount,
                            RemainingBalance = remainingBalance,
                            DisplayText = $"{donor.Name} (Available: BDT {remainingBalance:F2})"
                        });
                    }
                }

                var projects = await _projectService.GetAllProjectsAsync();
                var availableProjects = new List<object>();
                foreach (var project in projects.Where(p => p.Status != "Completed" && p.Status != "Cancelled"))
                {
                    // Refresh project data to get updated allocated amounts
                    await _projectService.UpdateProjectAllocatedAmountAsync(project.Id);
                    var updatedProject = await _projectService.GetProjectByIdAsync(project.Id);
                    
                    availableProjects.Add(new
                    {
                        Id = updatedProject.Id,
                        Name = updatedProject.Name,
                        Category = updatedProject.Category,
                        RequiredAmount = updatedProject.RequiredAmount,
                        AllocatedAmount = updatedProject.AllocatedAmount,
                        RemainingAmount = updatedProject.RemainingAmount
                    });
                }

                ViewBag.AvailableDonors = donorsWithBalance;
                ViewBag.AvailableProjects = availableProjects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allocation form data");
            }
        }
    }
}
