using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms;

namespace Sector_13_Welfare_Society___Digital_Management_System.Services
{
    public interface IProjectManagementService
    {
        Task<Project> CreateProjectAsync(ProjectCreationRequest request, string createdBy);
        Task<Project?> GetProjectByIdAsync(int id);
        Task<List<Project>> GetAllProjectsAsync();
        Task<List<Project>> GetPublicProjectsAsync();
        Task<Project?> UpdateProjectAsync(int id, ProjectCreationRequest request, string updatedBy);
        Task<bool> DeleteProjectAsync(int id);
        Task<ProjectFundingViewModel?> GetProjectFundingDetailsAsync(int projectId);
        Task<List<ProjectFundingViewModel>> GetAllProjectFundingDetailsAsync();
        Task<ProjectProgress> AddProjectProgressAsync(ProjectProgressUpdate update, string updatedBy);
        Task<List<ProjectProgress>> GetProjectProgressAsync(int projectId);
        Task<decimal> CalculateProjectFundingPercentageAsync(int projectId);
        Task<decimal> CalculateTotalAllocatedAmountAsync(int projectId);
        Task<bool> UpdateProjectAllocatedAmountAsync(int projectId);
    }

    public class ProjectManagementService : IProjectManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjectManagementService> _logger;

        public ProjectManagementService(ApplicationDbContext context, ILogger<ProjectManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Project> CreateProjectAsync(ProjectCreationRequest request, string createdBy)
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                RequiredAmount = request.RequiredAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Category = request.Category,
                Location = request.Location,
                ProjectManager = request.ProjectManager,
                Objectives = request.Objectives,
                ExpectedOutcomes = request.ExpectedOutcomes,
                IsPublic = request.IsPublic,
                CreatedBy = createdBy,
                CreatedDate = DateTime.Now,
                Status = "Planning"
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Project created: {ProjectId} by {CreatedBy}", project.Id, createdBy);
            return project;
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.DonationAllocations)
                    .ThenInclude(da => da.Donor)
                .Include(p => p.ProjectProgresses)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.DonationAllocations)
                .Include(p => p.ProjectProgresses)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Project>> GetPublicProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.DonationAllocations)
                .Where(p => p.IsPublic)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<Project?> UpdateProjectAsync(int id, ProjectCreationRequest request, string updatedBy)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return null;

            project.Name = request.Name;
            project.Description = request.Description;
            project.RequiredAmount = request.RequiredAmount;
            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;
            project.Category = request.Category;
            project.Location = request.Location;
            project.ProjectManager = request.ProjectManager;
            project.Objectives = request.Objectives;
            project.ExpectedOutcomes = request.ExpectedOutcomes;
            project.IsPublic = request.IsPublic;
            project.LastUpdated = DateTime.Now;
            project.LastUpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Project updated: {ProjectId} by {UpdatedBy}", project.Id, updatedBy);
            return project;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Project deleted: {ProjectId}", id);
            return true;
        }

        public async Task<ProjectFundingViewModel?> GetProjectFundingDetailsAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.DonationAllocations)
                    .ThenInclude(da => da.Donor)
                .Include(p => p.ProjectProgresses)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return null;

            var fundingPercentage = await CalculateProjectFundingPercentageAsync(projectId);

            var topDonors = project.DonationAllocations
                .Where(da => da.Donor != null)
                .OrderByDescending(da => da.Amount)
                .Take(5)
                .Select(da => new ProjectDonorDetail
                {
                    DonorId = da.DonorId,
                    DonorName = da.Donor!.Name,
                    Amount = da.Amount,
                    AllocationDate = da.AllocationDate,
                    Status = da.Status
                })
                .ToList();

            var recentProgress = project.ProjectProgresses
                .OrderByDescending(pp => pp.UpdateDate)
                .Take(3)
                .ToList();

            return new ProjectFundingViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                ProjectCategory = project.Category,
                Description = project.Description,
                RequiredAmount = project.RequiredAmount,
                AllocatedAmount = project.AllocatedAmount,
                RemainingAmount = project.RemainingAmount,
                FundingPercentage = fundingPercentage,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedDate = project.CreatedDate,
                ProjectManager = project.ProjectManager ?? "",
                IsPublic = project.IsPublic,
                TopDonors = topDonors,
                RecentProgress = recentProgress
            };
        }

        public async Task<List<ProjectFundingViewModel>> GetAllProjectFundingDetailsAsync()
        {
            var projects = await _context.Projects
                .Include(p => p.DonationAllocations)
                .Include(p => p.ProjectProgresses)
                .ToListAsync();

            var result = new List<ProjectFundingViewModel>();

            foreach (var project in projects)
            {
                var fundingPercentage = await CalculateProjectFundingPercentageAsync(project.Id);

                var topDonors = project.DonationAllocations
                    .Where(da => da.Donor != null)
                    .OrderByDescending(da => da.Amount)
                    .Take(3)
                    .Select(da => new ProjectDonorDetail
                    {
                        DonorId = da.DonorId,
                        DonorName = da.Donor!.Name,
                        Amount = da.Amount,
                        AllocationDate = da.AllocationDate,
                        Status = da.Status
                    })
                    .ToList();

                result.Add(new ProjectFundingViewModel
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    ProjectCategory = project.Category,
                    Description = project.Description,
                    RequiredAmount = project.RequiredAmount,
                    AllocatedAmount = project.AllocatedAmount,
                    RemainingAmount = project.RemainingAmount,
                    FundingPercentage = fundingPercentage,
                    Status = project.Status,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    CreatedDate = project.CreatedDate,
                    ProjectManager = project.ProjectManager ?? "",
                    IsPublic = project.IsPublic,
                    TopDonors = topDonors,
                    RecentProgress = project.ProjectProgresses.OrderByDescending(pp => pp.UpdateDate).Take(2).ToList()
                });
            }

            return result.OrderByDescending(p => p.CreatedDate).ToList();
        }

        public async Task<ProjectProgress> AddProjectProgressAsync(ProjectProgressUpdate update, string updatedBy)
        {
            var progress = new ProjectProgress
            {
                ProjectId = update.ProjectId,
                Title = update.Title,
                Description = update.Description,
                ProgressPercentage = update.ProgressPercentage,
                AmountUtilized = update.AmountUtilized,
                Category = update.Category,
                Status = update.Status,
                Challenges = update.Challenges,
                NextSteps = update.NextSteps,
                IsPublic = update.IsPublic,
                UpdatedBy = updatedBy,
                UpdateDate = DateTime.Now
            };

            _context.ProjectProgresses.Add(progress);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Project progress added for project: {ProjectId} by {UpdatedBy}", update.ProjectId, updatedBy);
            return progress;
        }

        public async Task<List<ProjectProgress>> GetProjectProgressAsync(int projectId)
        {
            return await _context.ProjectProgresses
                .Where(pp => pp.ProjectId == projectId)
                .OrderByDescending(pp => pp.UpdateDate)
                .ToListAsync();
        }

        public async Task<decimal> CalculateProjectFundingPercentageAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null || project.RequiredAmount <= 0) return 0;

            var allocatedAmount = await CalculateTotalAllocatedAmountAsync(projectId);
            return Math.Round((allocatedAmount / project.RequiredAmount) * 100, 2);
        }

        public async Task<decimal> CalculateTotalAllocatedAmountAsync(int projectId)
        {
            return await _context.DonationAllocations
                .Where(da => da.ProjectId == projectId && da.Status == "Allocated")
                .SumAsync(da => da.Amount);
        }

        public async Task<bool> UpdateProjectAllocatedAmountAsync(int projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;

            var totalAllocated = await CalculateTotalAllocatedAmountAsync(projectId);
            project.AllocatedAmount = totalAllocated;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
