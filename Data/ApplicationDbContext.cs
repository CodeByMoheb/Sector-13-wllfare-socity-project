using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
        public DbSet<Donor> Donors { get; set; }
<<<<<<< Updated upstream
=======
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveEntitlementPolicy> LeaveEntitlementPolicies { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<PermanentMember> PermanentMembers { get; set; }
        public DbSet<LeadershipMessage> LeadershipMessages { get; set; }
        public DbSet<ElectedCandidate> ElectedCandidates { get; set; }
        public DbSet<PreviousCandidate> PreviousCandidates { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<DonationAllocation> DonationAllocations { get; set; }
        public DbSet<ProjectProgress> ProjectProgresses { get; set; }
>>>>>>> Stashed changes
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure schema for all tables - switch to 'zidan'
            builder.HasDefaultSchema("zidan");
            
            // Configure ApprovalRequest
            builder.Entity<ApprovalRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.RequestType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RequestedBy).IsRequired();
                entity.Property(e => e.RequestedByName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RequestDate).IsRequired();
                entity.Property(e => e.Status).IsRequired();
            });
            
            // Configure Donor
            builder.Entity<Donor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.PaymentStatus).HasMaxLength(20);
                entity.Property(e => e.DonationDate).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.DonationType).HasMaxLength(50);
                entity.Property(e => e.ReceiptNumber).HasMaxLength(50);
            });
<<<<<<< Updated upstream
=======

            // Configure Employee
            builder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn(); // Ensure Id is auto-generated
                entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BaseSalary).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
            });

            // Configure Attendance
            builder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.AttendanceId);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Remarks).HasMaxLength(500);
            });

            // Configure Shift
            builder.Entity<Shift>(entity =>
            {
                entity.HasKey(e => e.ShiftId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            // Configure Leave
            builder.Entity<Leave>(entity =>
            {
                entity.HasKey(e => e.LeaveId);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.ApprovalStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ApprovalRemarks).HasMaxLength(200);
            });

            // Configure LeaveBalance
            builder.Entity<LeaveBalance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.TotalEntitled).IsRequired();
                entity.Property(e => e.Used).HasDefaultValue(0);
                entity.Property(e => e.Pending).HasDefaultValue(0);
                
                // Create unique constraint for employee, year, and leave type
                entity.HasIndex(e => new { e.EmployeeId, e.Year, e.LeaveType }).IsUnique();
                
                // Configure foreign key relationship
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure LeaveEntitlementPolicy
            builder.Entity<LeaveEntitlementPolicy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DefaultEntitled).IsRequired();
                entity.HasIndex(e => e.LeaveType).IsUnique();
            });

            // Configure LeadershipMessage
            builder.Entity<LeadershipMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Designation).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.MessageType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedBy).IsRequired();
            });

            // Configure ElectedCandidate
            builder.Entity<ElectedCandidate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Designation).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.ElectionYear).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedBy).IsRequired();
            });

            // Configure PreviousCandidate
            builder.Entity<PreviousCandidate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Designation).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.TermPeriod).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedBy).IsRequired();
            });

            // Configure Project
            builder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.RequiredAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.AllocatedAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.ProjectManager).HasMaxLength(100);
                entity.Property(e => e.Objectives).HasMaxLength(500);
                entity.Property(e => e.ExpectedOutcomes).HasMaxLength(500);
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
            });

            // Configure DonationAllocation
            builder.Entity<DonationAllocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DonorId).IsRequired();
                entity.Property(e => e.ProjectId).IsRequired();
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.AllocationDate).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Purpose).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.AllocatedBy).IsRequired();
                entity.Property(e => e.UtilizationDetails).HasMaxLength(500);

                // Configure foreign key relationships
                entity.HasOne(e => e.Donor)
                      .WithMany()
                      .HasForeignKey(e => e.DonorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.DonationAllocations)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure ProjectProgress
            builder.Entity<ProjectProgress>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProjectId).IsRequired();
                entity.Property(e => e.UpdateDate).IsRequired();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.ProgressPercentage).IsRequired();
                entity.Property(e => e.AmountUtilized).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Category).HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(100);
                entity.Property(e => e.Challenges).HasMaxLength(500);
                entity.Property(e => e.NextSteps).HasMaxLength(500);
                entity.Property(e => e.Images).HasMaxLength(500);
                entity.Property(e => e.UpdatedBy).IsRequired();

                // Configure foreign key relationship
                entity.HasOne(e => e.Project)
                      .WithMany(p => p.ProjectProgresses)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
>>>>>>> Stashed changes
        }
    }
}
