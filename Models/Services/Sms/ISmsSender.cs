using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms
{
    public interface ISmsSender
    {
        Task<bool> SendAsync(string phoneNumber, string message);
        Task<bool> SendBulkAsync(string[] phoneNumbers, string message);
    }
}


