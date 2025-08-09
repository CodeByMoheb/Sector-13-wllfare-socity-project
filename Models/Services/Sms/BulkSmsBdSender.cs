using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms
{
    public class BulkSmsBdSender : ISmsSender
    {
        private readonly IConfiguration _configuration;
        private static readonly HttpClient Http = new HttpClient();

        public BulkSmsBdSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendAsync(string phoneNumber, string message)
        {
            var apiKey = _configuration["SmsSettings:ApiKey"] ?? string.Empty;
            var sender = _configuration["SmsSettings:SenderId"] ?? string.Empty;
            var baseUrl = _configuration["SmsSettings:BaseUrl"] ?? "https://bulksmsbd.net/api/smsapi";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(sender))
            {
                return false;
            }

            var url = $"{baseUrl}?api_key={Uri.EscapeDataString(apiKey)}&type=text&number={Uri.EscapeDataString(phoneNumber)}&senderid={Uri.EscapeDataString(sender)}&message={Uri.EscapeDataString(message)}";
            var resp = await Http.GetAsync(url);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> SendBulkAsync(string[] phoneNumbers, string message)
        {
            var apiKey = _configuration["SmsSettings:ApiKey"] ?? string.Empty;
            var sender = _configuration["SmsSettings:SenderId"] ?? string.Empty;
            var baseUrl = _configuration["SmsSettings:BaseUrl"] ?? "https://bulksmsbd.net/api/smsapi";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(sender) || phoneNumbers.Length == 0)
            {
                return false;
            }

            var numbers = string.Join(",", phoneNumbers);
            var url = $"{baseUrl}?api_key={Uri.EscapeDataString(apiKey)}&type=text&number={Uri.EscapeDataString(numbers)}&senderid={Uri.EscapeDataString(sender)}&message={Uri.EscapeDataString(message)}";
            var resp = await Http.GetAsync(url);
            return resp.IsSuccessStatusCode;
        }
    }
}


