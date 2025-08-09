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

        private static string? NormalizeBdMsisdn(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var digits = new string(input.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("8801") && digits.Length == 13) return digits;
            if (digits.StartsWith("01") && digits.Length == 11) return "88" + digits;
            if (digits.StartsWith("1") && digits.Length == 10) return "880" + digits;
            if (digits.StartsWith("+8801") && digits.Length == 14) return digits.TrimStart('+');
            return null;
        }

        private static bool LooksSuccessful(string responseText)
        {
            if (string.IsNullOrEmpty(responseText)) return false;
            var t = responseText.ToLowerInvariant();
            if (t.Contains("success") || t.Contains("sms sent") || t.Contains("ok")) return true;
            if (t.Contains("error") || t.Contains("invalid") || t.Contains("failed")) return false;
            return true; // assume success when HTTP 200 and no explicit error keywords
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

            var msisdn = NormalizeBdMsisdn(phoneNumber);
            if (msisdn == null) return false;

            var url = $"{baseUrl}?api_key={Uri.EscapeDataString(apiKey)}&type=text&number={Uri.EscapeDataString(msisdn)}&senderid={Uri.EscapeDataString(sender)}&message={Uri.EscapeDataString(message)}";
            var resp = await Http.GetAsync(url);
            var content = await resp.Content.ReadAsStringAsync();
            return resp.IsSuccessStatusCode && LooksSuccessful(content);
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

            var normalized = phoneNumbers
                .Select(NormalizeBdMsisdn)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToArray();
            if (normalized.Length == 0) return false;

            var numbers = string.Join(",", normalized);
            var url = $"{baseUrl}?api_key={Uri.EscapeDataString(apiKey)}&type=text&number={Uri.EscapeDataString(numbers)}&senderid={Uri.EscapeDataString(sender)}&message={Uri.EscapeDataString(message)}";
            var resp = await Http.GetAsync(url);
            var content = await resp.Content.ReadAsStringAsync();
            return resp.IsSuccessStatusCode && LooksSuccessful(content);
        }
    }
}


