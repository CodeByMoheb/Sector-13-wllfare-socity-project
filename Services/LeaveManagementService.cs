using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Services
{
    public class LeaveManagementService : ILeaveManagementService
    {
        private readonly ApplicationDbContext _context;

        public LeaveManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task InitializeLeaveBalancesForEmployee(int employeeId, int year)
        {
            try
            {
                // Check if employee exists (use EF, more reliable than raw scalar SQL here)
                var employeeExists = await _context.Employees.AnyAsync(e => e.Id == employeeId);
                if (!employeeExists) return;

                // Load policy-driven entitlements
                var entitlements = await _context.LeaveEntitlementPolicies
                    .Where(p => p.IsActive)
                    .ToDictionaryAsync(p => p.LeaveType, p => p.DefaultEntitled);
                if (!entitlements.Any())
                {
                    // Fallback: seed a minimal policy if none exists
                    entitlements = new Dictionary<string, int>
                    {
                        ["Annual Leave"] = 21,
                        ["Casual Leave"] = 10,
                        ["Sick Leave"] = 14,
                        ["Emergency Leave"] = 5,
                        ["Paternity Leave"] = 7,
                        ["Maternity Leave"] = 112,
                        ["Religious Leave"] = 3
                    };
                }

                // Insert leave balances via EF for robustness
                foreach (var entitlement in entitlements)
                {
                    var exists = await _context.LeaveBalances.AnyAsync(lb => lb.EmployeeId == employeeId && lb.Year == year && lb.LeaveType == entitlement.Key);
                    if (!exists)
                    {
                        _context.LeaveBalances.Add(new LeaveBalance
                        {
                            EmployeeId = employeeId,
                            Year = year,
                            LeaveType = entitlement.Key,
                            TotalEntitled = entitlement.Value,
                            Used = 0,
                            Pending = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Error initializing leave balances: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Don't throw - allow the application to continue even if initialization fails
                System.Diagnostics.Debug.WriteLine("Continuing without leave balance initialization...");
            }
        }

        public async Task<List<LeaveBalance>> GetEmployeeLeaveBalances(int employeeId, int year)
        {
            var balances = await _context.LeaveBalances
                .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
                .OrderBy(lb => lb.LeaveType)
                .ToListAsync();

            // If no balances found, try to initialize them
            if (!balances.Any())
            {
                await InitializeLeaveBalancesForEmployee(employeeId, year);
                balances = await _context.LeaveBalances
                    .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
                    .OrderBy(lb => lb.LeaveType)
                    .ToListAsync();
            }

            return balances;
        }

        public async Task<LeaveBalance?> GetLeaveBalance(int employeeId, int year, string leaveType)
        {
            return await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && 
                                          lb.Year == year && 
                                          lb.LeaveType == leaveType);
        }

        public async Task<bool> CanApplyForLeave(int employeeId, string leaveType, int daysRequested)
        {
            var currentYear = DateTime.Now.Year;
            var balance = await GetLeaveBalance(employeeId, currentYear, leaveType);
            
            if (balance == null)
            {
                // Initialize balances if not exists
                await InitializeLeaveBalancesForEmployee(employeeId, currentYear);
                balance = await GetLeaveBalance(employeeId, currentYear, leaveType);
            }

            return balance != null && balance.Remaining >= daysRequested;
        }

        public async Task<bool> ApplyForLeave(Leave leave)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[LeaveService] ApplyForLeave called - EmployeeId: {leave.EmployeeId}, LeaveType: {leave.LeaveType}, Days: {leave.NumberOfDays}");
                
                var currentYear = DateTime.Now.Year;
                
                // First, ensure leave balances exist for this employee
                await InitializeLeaveBalancesForEmployee(leave.EmployeeId, currentYear);
                
                // Ensure policy exists for the requested leave type
                var policy = await _context.LeaveEntitlementPolicies.FirstOrDefaultAsync(p => p.LeaveType == leave.LeaveType && p.IsActive);
                if (policy == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LeaveService] No active policy found for {leave.LeaveType}");
                    return false;
                }
                
                // Fetch current balance row and validate again atomically
                var balance = await _context.LeaveBalances
                    .FirstOrDefaultAsync(b => b.EmployeeId == leave.EmployeeId && b.Year == currentYear && b.LeaveType == leave.LeaveType);
                if (balance == null)
                {
                    // Create missing balances then refetch
                    await InitializeLeaveBalancesForEmployee(leave.EmployeeId, currentYear);
                    balance = await _context.LeaveBalances
                        .FirstOrDefaultAsync(b => b.EmployeeId == leave.EmployeeId && b.Year == currentYear && b.LeaveType == leave.LeaveType);
                }
                if (balance == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LeaveService] ERROR: Balance row missing after initialization");
                    return false;
                }
                if ((balance.TotalEntitled - balance.Used - balance.Pending) < leave.NumberOfDays)
                {
                    System.Diagnostics.Debug.WriteLine($"[LeaveService] Insufficient balance at commit time");
                    return false;
                }

                // Use raw SQL to update balance and insert leave record to avoid EF tracking issues
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Update pending balance
                    balance.Pending += leave.NumberOfDays;
                    balance.UpdatedAt = DateTime.UtcNow;
                    _context.LeaveBalances.Update(balance);

                    // Insert leave record
                    _context.Leaves.Add(leave);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LeaveService] ERROR applying for leave: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LeaveService] Stack trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"[LeaveService] Leave details - EmployeeId: {leave.EmployeeId}, LeaveType: {leave.LeaveType}, Days: {leave.NumberOfDays}");
                return false;
            }
        }

        public async Task<bool> ApproveLeave(int leaveId, string approvedById, string? remarks = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var leave = await _context.Leaves.FindAsync(leaveId);
                if (leave == null || leave.ApprovalStatus != "Pending") return false;

                var currentYear = DateTime.Now.Year;
                var balance = await GetLeaveBalance(leave.EmployeeId, currentYear, leave.LeaveType);
                
                if (balance != null)
                {
                    // Move from pending to used
                    balance.Pending -= leave.NumberOfDays;
                    balance.Used += leave.NumberOfDays;
                    balance.UpdatedAt = DateTime.UtcNow;
                    _context.LeaveBalances.Update(balance);
                }

                // Update leave status
                leave.ApprovalStatus = "Approved";
                leave.ApprovedById = approvedById;
                leave.ApprovalDate = DateTime.UtcNow;
                leave.ApprovalRemarks = remarks;
                leave.UpdatedAt = DateTime.UtcNow;
                _context.Leaves.Update(leave);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> RejectLeave(int leaveId, string approvedById, string? remarks = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var leave = await _context.Leaves.FindAsync(leaveId);
                if (leave == null || leave.ApprovalStatus != "Pending") return false;

                var currentYear = DateTime.Now.Year;
                var balance = await GetLeaveBalance(leave.EmployeeId, currentYear, leave.LeaveType);
                
                if (balance != null)
                {
                    // Remove from pending (return to available)
                    balance.Pending -= leave.NumberOfDays;
                    balance.UpdatedAt = DateTime.UtcNow;
                    _context.LeaveBalances.Update(balance);
                }

                // Update leave status
                leave.ApprovalStatus = "Rejected";
                leave.ApprovedById = approvedById;
                leave.ApprovalDate = DateTime.UtcNow;
                leave.ApprovalRemarks = remarks;
                leave.UpdatedAt = DateTime.UtcNow;
                _context.Leaves.Update(leave);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<LeaveBalance>> GetAllEmployeeBalances(int year)
        {
            return await _context.LeaveBalances
                .Include(lb => lb.Employee)
                .Where(lb => lb.Year == year)
                .OrderBy(lb => lb.Employee.Name)
                .ThenBy(lb => lb.LeaveType)
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetLeaveStatistics(int year)
        {
            var balances = await GetAllEmployeeBalances(year);
            var leaves = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.StartDate.Year == year)
                .ToListAsync();

            var stats = new Dictionary<string, object>
            {
                ["TotalEmployees"] = balances.Select(b => b.EmployeeId).Distinct().Count(),
                ["TotalLeaveRequests"] = leaves.Count,
                ["ApprovedLeaves"] = leaves.Count(l => l.ApprovalStatus == "Approved"),
                ["PendingLeaves"] = leaves.Count(l => l.ApprovalStatus == "Pending"),
                ["RejectedLeaves"] = leaves.Count(l => l.ApprovalStatus == "Rejected"),
                ["TotalLeaveDaysUsed"] = balances.Sum(b => b.Used),
                ["TotalLeaveDaysPending"] = balances.Sum(b => b.Pending),
                ["ByLeaveType"] = balances.GroupBy(b => b.LeaveType)
                    .ToDictionary(g => g.Key, g => new
                    {
                        TotalEntitled = g.Sum(b => b.TotalEntitled),
                        Used = g.Sum(b => b.Used),
                        Pending = g.Sum(b => b.Pending),
                        Remaining = g.Sum(b => b.Remaining)
                    })
            };

            return stats;
        }
    }
}
