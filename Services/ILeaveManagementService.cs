using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Services
{
    public interface ILeaveManagementService
    {
        Task InitializeLeaveBalancesForEmployee(int employeeId, int year);
        Task<List<LeaveBalance>> GetEmployeeLeaveBalances(int employeeId, int year);
        Task<LeaveBalance?> GetLeaveBalance(int employeeId, int year, string leaveType);
        Task<bool> CanApplyForLeave(int employeeId, string leaveType, int daysRequested);
        Task<bool> ApplyForLeave(Leave leave);
        Task<bool> ApproveLeave(int leaveId, string approvedById, string? remarks = null);
        Task<bool> RejectLeave(int leaveId, string approvedById, string? remarks = null);
        Task<List<LeaveBalance>> GetAllEmployeeBalances(int year);
        Task<Dictionary<string, object>> GetLeaveStatistics(int year);
    }
}
