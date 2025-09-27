using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms;

namespace Sector_13_Welfare_Society___Digital_Management_System.Services
{
    public interface IDonationAllocationService
    {
        Task<DonationAllocation> AllocateDonationAsync(DonationAllocationRequest request, string allocatedBy);
        Task<List<DonationAllocation>> GetDonorAllocationsAsync(int donorId);
        Task<List<DonationAllocation>> GetProjectAllocationsAsync(int projectId);
        Task<DonationAllocation?> GetAllocationByIdAsync(int id);
        Task<bool> UpdateAllocationStatusAsync(int allocationId, string status, string? utilizationDetails = null, string? utilizedBy = null);
        Task<ContributionTrackingViewModel?> GetDonorContributionTrackingAsync(int donorId);
        Task<List<ContributionTrackingViewModel>> GetAllDonorContributionsAsync();
        Task<decimal> GetDonorTotalDonatedAsync(int donorId);
        Task<decimal> GetDonorTotalAllocatedAsync(int donorId);
        Task<decimal> GetDonorRemainingBalanceAsync(int donorId);
        Task<bool> CanAllocateAmountAsync(int donorId, decimal amount);
        Task SendAllocationNotificationAsync(DonationAllocation allocation);
    }

    public class DonationAllocationService : IDonationAllocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISmsSender _smsSender;
        private readonly IEmailService _emailService;
        private readonly IProjectManagementService _projectService;
        private readonly ILogger<DonationAllocationService> _logger;

        public DonationAllocationService(
            ApplicationDbContext context,
            ISmsSender smsSender,
            IEmailService emailService,
            IProjectManagementService projectService,
            ILogger<DonationAllocationService> logger)
        {
            _context = context;
            _smsSender = smsSender;
            _emailService = emailService;
            _projectService = projectService;
            _logger = logger;
        }

        public async Task<DonationAllocation> AllocateDonationAsync(DonationAllocationRequest request, string allocatedBy)
        {
            // Validate that the donor exists and has sufficient balance
            if (!await CanAllocateAmountAsync(request.DonorId, request.Amount))
            {
                throw new InvalidOperationException("Insufficient donor balance for allocation");
            }

            var allocation = new DonationAllocation
            {
                DonorId = request.DonorId,
                ProjectId = request.ProjectId,
                Amount = request.Amount,
                Purpose = request.Purpose,
                Notes = request.Notes,
                AllocatedBy = allocatedBy,
                AllocationDate = DateTime.Now,
                Status = "Allocated"
            };

            _context.DonationAllocations.Add(allocation);
            await _context.SaveChangesAsync();

            // Update project allocated amount
            await _projectService.UpdateProjectAllocatedAmountAsync(request.ProjectId);

            // Send notification to donor
            await SendAllocationNotificationAsync(allocation);

            _logger.LogInformation("Donation allocated: {AllocationId} for donor {DonorId} to project {ProjectId}", 
                allocation.Id, request.DonorId, request.ProjectId);

            return allocation;
        }

        public async Task<List<DonationAllocation>> GetDonorAllocationsAsync(int donorId)
        {
            return await _context.DonationAllocations
                .Include(da => da.Project)
                .Where(da => da.DonorId == donorId)
                .OrderByDescending(da => da.AllocationDate)
                .ToListAsync();
        }

        public async Task<List<DonationAllocation>> GetProjectAllocationsAsync(int projectId)
        {
            return await _context.DonationAllocations
                .Include(da => da.Donor)
                .Where(da => da.ProjectId == projectId)
                .OrderByDescending(da => da.AllocationDate)
                .ToListAsync();
        }

        public async Task<DonationAllocation?> GetAllocationByIdAsync(int id)
        {
            return await _context.DonationAllocations
                .Include(da => da.Donor)
                .Include(da => da.Project)
                .FirstOrDefaultAsync(da => da.Id == id);
        }

        public async Task<bool> UpdateAllocationStatusAsync(int allocationId, string status, string? utilizationDetails = null, string? utilizedBy = null)
        {
            var allocation = await _context.DonationAllocations.FindAsync(allocationId);
            if (allocation == null) return false;

            allocation.Status = status;
            if (utilizationDetails != null)
                allocation.UtilizationDetails = utilizationDetails;
            if (utilizedBy != null)
                allocation.UtilizedBy = utilizedBy;
            if (status == "Utilized")
                allocation.UtilizedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Allocation status updated: {AllocationId} to {Status}", allocationId, status);
            return true;
        }

        public async Task<ContributionTrackingViewModel?> GetDonorContributionTrackingAsync(int donorId)
        {
            var donor = await _context.Donors.FindAsync(donorId);
            if (donor == null) return null;

            var allocations = await _context.DonationAllocations
                .Include(da => da.Project)
                .Where(da => da.DonorId == donorId)
                .OrderByDescending(da => da.AllocationDate)
                .ToListAsync();

            var totalAllocated = allocations.Sum(da => da.Amount);
            var remainingBalance = await GetDonorRemainingBalanceAsync(donorId);

            var allocationDetails = allocations.Select(da => new DonationAllocationDetail
            {
                AllocationId = da.Id,
                ProjectId = da.ProjectId,
                ProjectName = da.Project.Name,
                ProjectCategory = da.Project.Category,
                Amount = da.Amount,
                AllocationDate = da.AllocationDate,
                Status = da.Status,
                Purpose = da.Purpose ?? "",
                ProgressPercentage = 0, // This would need to be calculated from project progress
                UtilizedDate = da.UtilizedDate
            }).ToList();

            return new ContributionTrackingViewModel
            {
                DonorId = donor.Id,
                DonorName = donor.Name,
                DonorEmail = donor.Email,
                DonorPhone = donor.Phone,
                TotalDonated = donor.Amount,
                TotalAllocated = totalAllocated,
                RemainingBalance = remainingBalance,
                LastDonationDate = donor.DonationDate,
                Allocations = allocationDetails
            };
        }

        public async Task<List<ContributionTrackingViewModel>> GetAllDonorContributionsAsync()
        {
            var donors = await _context.Donors
                .Where(d => d.PaymentStatus == "Completed")
                .ToListAsync();

            var result = new List<ContributionTrackingViewModel>();

            foreach (var donor in donors)
            {
                var tracking = await GetDonorContributionTrackingAsync(donor.Id);
                if (tracking != null)
                    result.Add(tracking);
            }

            return result.OrderByDescending(c => c.TotalDonated).ToList();
        }

        public async Task<decimal> GetDonorTotalDonatedAsync(int donorId)
        {
            var donor = await _context.Donors.FindAsync(donorId);
            return donor?.Amount ?? 0;
        }

        public async Task<decimal> GetDonorTotalAllocatedAsync(int donorId)
        {
            return await _context.DonationAllocations
                .Where(da => da.DonorId == donorId)
                .SumAsync(da => da.Amount);
        }

        public async Task<decimal> GetDonorRemainingBalanceAsync(int donorId)
        {
            var donor = await _context.Donors.FindAsync(donorId);
            if (donor == null || donor.PaymentStatus != "Completed")
                return 0;

            var totalAllocated = await _context.DonationAllocations
                .Where(da => da.DonorId == donorId)
                .SumAsync(da => da.Amount);

            return Math.Max(0, donor.Amount - totalAllocated);
        }

        public async Task<bool> CanAllocateAmountAsync(int donorId, decimal amount)
        {
            var remainingBalance = await GetDonorRemainingBalanceAsync(donorId);
            return remainingBalance >= amount;
        }

        public async Task SendAllocationNotificationAsync(DonationAllocation allocation)
        {
            try
            {
                var donor = await _context.Donors.FindAsync(allocation.DonorId);
                var project = await _context.Projects.FindAsync(allocation.ProjectId);
                
                if (donor == null || project == null) return;

                var fundingPercentage = await _projectService.CalculateProjectFundingPercentageAsync(allocation.ProjectId);

                // SMS Notification
                if (!string.IsNullOrWhiteSpace(donor.Phone))
                {
                    var donorRemainingBalance = await GetDonorRemainingBalanceAsync(donor.Id);
                    var smsMessage = $"Donation BDT {allocation.Amount:F2} allocated to '{project.Name}'. " +
                                   $"Project: {fundingPercentage:F1}% funded. " +
                                   $"Your balance: BDT {donorRemainingBalance:F2}. " +
                                   $"Thank you! - Sector 13 Welfare Society";
                    
                    await _smsSender.SendAsync(donor.Phone, smsMessage);
                    _logger.LogInformation("Allocation SMS sent to donor {DonorId}", donor.Id);
                }

                // Email Notification
                if (!string.IsNullOrWhiteSpace(donor.Email))
                {
                    var donorRemainingBalance = await GetDonorRemainingBalanceAsync(donor.Id);
                    var subject = $"Donation Allocation Update - {project.Name}";
                    var body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='color: #2c5aa0;'>Donation Allocation Update</h2>
                                
                                <p>Dear {donor.Name},</p>
                                
                                <p>We are pleased to inform you that your generous donation has been allocated to support our cause.</p>
                                
                                <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                    <h3 style='margin-top: 0; color: #2c5aa0;'>Allocation Details</h3>
                                    <p><strong>Project:</strong> {project.Name}</p>
                                    <p><strong>Amount Allocated:</strong> BDT {allocation.Amount:F2}</p>
                                    <p><strong>Purpose:</strong> {allocation.Purpose ?? "General project support"}</p>
                                    <p><strong>Allocation Date:</strong> {allocation.AllocationDate:dd MMMM yyyy}</p>
                                </div>
                                
                                <div style='background-color: #e8f4fd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                    <h3 style='margin-top: 0; color: #2c5aa0;'>Project Status</h3>
                                    <p><strong>Funding Progress:</strong> {fundingPercentage:F1}% complete</p>
                                    <p><strong>Amount Raised:</strong> BDT {project.AllocatedAmount:F2}</p>
                                    <p><strong>Target Amount:</strong> BDT {project.RequiredAmount:F2}</p>
                                    <p><strong>Remaining Needed:</strong> BDT {project.RemainingAmount:F2}</p>
                                </div>
                                
                                <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                    <h3 style='margin-top: 0; color: #856404;'>Your Donation Summary</h3>
                                    <p><strong>Total Donated:</strong> BDT {donor.Amount:F2}</p>
                                    <p><strong>Total Allocated:</strong> BDT {donor.Amount - donorRemainingBalance:F2}</p>
                                    <p><strong>Remaining Balance:</strong> BDT {donorRemainingBalance:F2}</p>
                                    <p><em>Your remaining balance can be allocated to other projects in the future.</em></p>
                                </div>
                                
                                <p>Your contribution is making a real difference in our community. We will keep you updated on the project's progress.</p>
                                
                                <p>Thank you for your continued support!</p>
                                
                                <p>Best regards,<br>
                                <strong>Sector 13 Welfare Society</strong></p>
                                
                                <hr style='margin: 30px 0; border: none; border-top: 1px solid #ddd;'>
                                <p style='font-size: 12px; color: #666;'>
                                    This is an automated notification. Please do not reply to this email.
                                </p>
                            </div>
                        </body>
                        </html>";

                    await _emailService.SendEmailAsync(donor.Email, subject, body);
                    _logger.LogInformation("Allocation email sent to donor {DonorId}", donor.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send allocation notification for allocation {AllocationId}", allocation.Id);
            }
        }
    }
}
