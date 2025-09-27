using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Services;
using System.Security.Claims;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Secretary,Admin")]
    public class ProjectManagementController : Controller
    {
        private readonly IProjectManagementService _projectService;
        private readonly IDonationAllocationService _allocationService;
        private readonly ILogger<ProjectManagementController> _logger;

        public ProjectManagementController(
            IProjectManagementService projectService,
            IDonationAllocationService allocationService,
            ILogger<ProjectManagementController> logger)
        {
            _projectService = projectService;
            _allocationService = allocationService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var projects = await _projectService.GetAllProjectFundingDetailsAsync();
                return View(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading projects");
                TempData["Error"] = "An error occurred while loading projects.";
                return View(new List<ProjectFundingViewModel>());
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> PublicProjects()
        {
            try
            {
                var projects = await _projectService.GetAllProjectFundingDetailsAsync();
                var publicProjects = projects.Where(p => p.Status != "Planning").ToList();
                return View(publicProjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public projects");
                TempData["Error"] = "An error occurred while loading projects.";
                return View(new List<ProjectFundingViewModel>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    TempData["Error"] = "Project not found.";
                    return RedirectToAction("Index");
                }

                var fundingDetails = await _projectService.GetProjectFundingDetailsAsync(id);
                var allocations = await _allocationService.GetProjectAllocationsAsync(id);
                var progress = await _projectService.GetProjectProgressAsync(id);

                ViewBag.Allocations = allocations;
                ViewBag.Progress = progress;
                ViewBag.FundingDetails = fundingDetails;

                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project details for project {ProjectId}", id);
                TempData["Error"] = "An error occurred while loading project details.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Create()
        {
            return View(new ProjectCreationRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                var project = await _projectService.CreateProjectAsync(request, userId);

                TempData["Success"] = $"Project '{project.Name}' has been created successfully.";
                return RedirectToAction("Details", new { id = project.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                ModelState.AddModelError("", "An error occurred while creating the project. Please try again.");
                return View(request);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    TempData["Error"] = "Project not found.";
                    return RedirectToAction("Index");
                }

                var request = new ProjectCreationRequest
                {
                    Name = project.Name,
                    Description = project.Description,
                    RequiredAmount = project.RequiredAmount,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Category = project.Category,
                    Location = project.Location,
                    ProjectManager = project.ProjectManager,
                    Objectives = project.Objectives,
                    ExpectedOutcomes = project.ExpectedOutcomes,
                    IsPublic = project.IsPublic
                };

                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading project for edit {ProjectId}", id);
                TempData["Error"] = "An error occurred while loading the project.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectCreationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                var project = await _projectService.UpdateProjectAsync(id, request, userId);

                if (project == null)
                {
                    TempData["Error"] = "Project not found.";
                    return RedirectToAction("Index");
                }

                TempData["Success"] = $"Project '{project.Name}' has been updated successfully.";
                return RedirectToAction("Details", new { id = project.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                ModelState.AddModelError("", "An error occurred while updating the project. Please try again.");
                return View(request);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _projectService.DeleteProjectAsync(id);
                if (success)
                {
                    TempData["Success"] = "Project has been deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Project not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                TempData["Error"] = "An error occurred while deleting the project.";
            }

            return RedirectToAction("Index");
        }

        public IActionResult AddProgress(int projectId)
        {
            ViewBag.ProjectId = projectId;
            return View(new ProjectProgressUpdate { ProjectId = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProgress(ProjectProgressUpdate update)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProjectId = update.ProjectId;
                return View(update);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                var progress = await _projectService.AddProjectProgressAsync(update, userId);

                TempData["Success"] = "Project progress has been updated successfully.";
                return RedirectToAction("Details", new { id = update.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding project progress for project {ProjectId}", update.ProjectId);
                ModelState.AddModelError("", "An error occurred while updating project progress. Please try again.");
                ViewBag.ProjectId = update.ProjectId;
                return View(update);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int projectId, string status)
        {
            try
            {
                var project = await _projectService.GetProjectByIdAsync(projectId);
                if (project == null)
                {
                    return Json(new { success = false, message = "Project not found." });
                }

                project.Status = status;
                project.LastUpdated = DateTime.Now;
                project.LastUpdatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

                // Save changes through the service
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
                var request = new ProjectCreationRequest
                {
                    Name = project.Name,
                    Description = project.Description,
                    RequiredAmount = project.RequiredAmount,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Category = project.Category,
                    Location = project.Location,
                    ProjectManager = project.ProjectManager,
                    Objectives = project.Objectives,
                    ExpectedOutcomes = project.ExpectedOutcomes,
                    IsPublic = project.IsPublic
                };

                await _projectService.UpdateProjectAsync(projectId, request, userId);

                return Json(new { success = true, message = $"Project status updated to {status}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project status for project {ProjectId}", projectId);
                return Json(new { success = false, message = "An error occurred while updating project status." });
            }
        }
    }
}
