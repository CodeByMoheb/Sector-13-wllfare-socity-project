using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Services;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [AllowAnonymous]
    public class ContributionController : Controller
    {
        private readonly IDonationAllocationService _allocationService;
        private readonly IProjectManagementService _projectService;
        private readonly ILogger<ContributionController> _logger;

        public ContributionController(
            IDonationAllocationService allocationService,
            IProjectManagementService projectService,
            ILogger<ContributionController> logger)
        {
            _allocationService = allocationService;
            _projectService = projectService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var projects = await _projectService.GetAllProjectFundingDetailsAsync();
                var publicProjects = projects.Where(p => p.IsPublic).ToList();
                
                // Calculate summary statistics
                var totalRequired = publicProjects.Sum(p => p.RequiredAmount);
                var totalAllocated = publicProjects.Sum(p => p.AllocatedAmount);
                var totalRemaining = publicProjects.Sum(p => p.RemainingAmount);
                var overallProgress = totalRequired > 0 ? Math.Round((totalAllocated / totalRequired) * 100, 2) : 0;

                ViewBag.TotalRequired = totalRequired;
                ViewBag.TotalAllocated = totalAllocated;
                ViewBag.TotalRemaining = totalRemaining;
                ViewBag.OverallProgress = overallProgress;
                ViewBag.TotalProjects = publicProjects.Count;
                ViewBag.ActiveProjects = publicProjects.Count(p => p.Status == "Active");

                return View(publicProjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contribution tracking page");
                TempData["Error"] = "An error occurred while loading contribution information.";
                return View(new List<ProjectFundingViewModel>());
            }
        }

        public async Task<IActionResult> ProjectDetails(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null || !project.IsPublic)
                {
                    TempData["Error"] = "Project not found or not available for public viewing.";
                    return RedirectToAction("Index");
                }

                var fundingDetails = await _projectService.GetProjectFundingDetailsAsync(id);
                var allocations = await _allocationService.GetProjectAllocationsAsync(id);
                var progress = await _projectService.GetProjectProgressAsync(id);

                // Filter to show only public allocations (hide donor details for privacy)
                var publicAllocations = allocations.Select(da => new
                {
                    Amount = da.Amount,
                    AllocationDate = da.AllocationDate,
                    Purpose = da.Purpose,
                    Status = da.Status
                }).ToList();

                ViewBag.Allocations = publicAllocations;
                ViewBag.Progress = progress.Where(p => p.IsPublic).ToList();
                ViewBag.FundingDetails = fundingDetails;

                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public project details for project {ProjectId}", id);
                TempData["Error"] = "An error occurred while loading project details.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> DonorLookup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchDonor(string email, string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
                {
                    ModelState.AddModelError("", "Please provide either email or phone number.");
                    return View("DonorLookup");
                }

                var donor = await _allocationService.GetAllDonorContributionsAsync();
                ContributionTrackingViewModel? matchingDonor = null;

                if (!string.IsNullOrWhiteSpace(email))
                {
                    matchingDonor = donor.FirstOrDefault(d => 
                        d.DonorEmail.Equals(email, StringComparison.OrdinalIgnoreCase));
                }
                else if (!string.IsNullOrWhiteSpace(phone))
                {
                    matchingDonor = donor.FirstOrDefault(d => 
                        d.DonorPhone.Equals(phone, StringComparison.OrdinalIgnoreCase));
                }

                if (matchingDonor == null)
                {
                    TempData["Info"] = "No donation records found for the provided information.";
                    return View("DonorLookup");
                }

                return View("DonorContributionDetails", matchingDonor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for donor");
                TempData["Error"] = "An error occurred while searching for donor information.";
                return View("DonorLookup");
            }
        }

        public async Task<IActionResult> ProjectProgress(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null || !project.IsPublic)
                {
                    TempData["Error"] = "Project not found or not available for public viewing.";
                    return RedirectToAction("Index");
                }

                var progress = await _projectService.GetProjectProgressAsync(id);
                var publicProgress = progress.Where(p => p.IsPublic).OrderByDescending(p => p.UpdateDate).ToList();

                ViewBag.Project = project;
                return View(publicProgress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project progress for project {ProjectId}", id);
                TempData["Error"] = "An error occurred while loading project progress.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Statistics()
        {
            try
            {
                var projects = await _projectService.GetAllProjectFundingDetailsAsync();
                var publicProjects = projects.Where(p => p.IsPublic).ToList();
                var contributions = await _allocationService.GetAllDonorContributionsAsync();

                var stats = new
                {
                    TotalProjects = publicProjects.Count,
                    ActiveProjects = publicProjects.Count(p => p.Status == "Active"),
                    CompletedProjects = publicProjects.Count(p => p.Status == "Completed"),
                    TotalRequired = publicProjects.Sum(p => p.RequiredAmount),
                    TotalAllocated = publicProjects.Sum(p => p.AllocatedAmount),
                    TotalRemaining = publicProjects.Sum(p => p.RemainingAmount),
                    TotalDonors = contributions.Count(),
                    TopCategories = publicProjects.GroupBy(p => p.ProjectCategory)
                        .Select(g => new { Category = g.Key, Count = g.Count(), Amount = g.Sum(p => p.AllocatedAmount) })
                        .OrderByDescending(x => x.Amount)
                        .Take(5)
                        .ToList(),
                    RecentProjects = publicProjects.OrderByDescending(p => p.StartDate).Take(5).ToList()
                };

                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contribution statistics");
                TempData["Error"] = "An error occurred while loading statistics.";
                return View(new { });
            }
        }
    }
}
