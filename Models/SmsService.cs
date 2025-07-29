using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class SmsService : ISmsService
    {
        private readonly SmsSettings _smsSettings;
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient _httpClient;

        public SmsService(IOptions<SmsSettings> smsSettings, ILogger<SmsService> logger, HttpClient httpClient)
        {
            _smsSettings = smsSettings.Value;
            _logger = logger;
            _httpClient = httpClient;
            
            _logger.LogInformation("SMS Service initialized - IsEnabled: {IsEnabled}, ApiKey: {ApiKey}, SenderId: {SenderId}", 
                _smsSettings.IsEnabled, _smsSettings.ApiKey, _smsSettings.SenderId);
        }

        public async Task<bool> IsSmsEnabledAsync()
        {
            var isEnabled = _smsSettings.IsEnabled;
            var hasApiKey = !string.IsNullOrEmpty(_smsSettings.ApiKey);
            
            _logger.LogInformation("SMS Service Check - IsEnabled: {IsEnabled}, HasApiKey: {HasApiKey}, ApiKey: {ApiKey}", 
                isEnabled, hasApiKey, _smsSettings.ApiKey);
            
            return isEnabled && hasApiKey;
        }

        public async Task<string> GetBalanceAsync()
        {
            if (!await IsSmsEnabledAsync())
            {
                return "SMS service not enabled";
            }

            try
            {
                var balanceUrl = $"http://bulksmsbd.net/api/getBalanceApi?api_key={Uri.EscapeDataString(_smsSettings.ApiKey)}";
                var response = await _httpClient.GetAsync(balanceUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Balance check response: {Response}", responseContent);
                
                // Try to parse the JSON response
                try
                {
                    var balanceData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (balanceData.TryGetProperty("balance", out var balanceElement))
                    {
                        var balance = balanceElement.GetDecimal();
                        var responseCode = balanceData.TryGetProperty("response_code", out var codeElement) ? codeElement.GetInt32() : 0;
                        
                        if (responseCode == 202) // Success code
                        {
                            if (balance < 0)
                            {
                                return $"⚠️ Negative Balance: {balance:F2} BDT - Please add funds to your account";
                            }
                            else
                            {
                                return $"💰 Available Balance: {balance:F2} BDT";
                            }
                        }
                        else
                        {
                            return $"❌ Error (Code: {responseCode}): {responseContent}";
                        }
                    }
                    else
                    {
                        return $"❌ Invalid response format: {responseContent}";
                    }
                }
                catch (JsonException)
                {
                    // If JSON parsing fails, return the raw response
                    return $"📊 Raw Response: {responseContent}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SMS balance");
                return $"❌ Error checking balance: {ex.Message}";
            }
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            if (!await IsSmsEnabledAsync())
            {
                _logger.LogWarning("SMS service is not enabled");
                return false;
            }

            try
            {
                var formattedPhone = FormatPhoneNumber(phoneNumber);
                _logger.LogInformation("Sending SMS to {OriginalPhone} -> {FormattedPhone}", phoneNumber, formattedPhone);

                switch (_smsSettings.Provider.ToLower())
                {
                    case "bulksmsbd":
                        return await SendViaBulkSmsBD(formattedPhone, message);
                    case "sslwireless":
                        return await SendViaSslWireless(formattedPhone, message);
                    default:
                        _logger.LogError("Unknown SMS provider: {Provider}", _smsSettings.Provider);
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendBulkSmsAsync(List<string> phoneNumbers, string message)
        {
            if (!await IsSmsEnabledAsync())
            {
                _logger.LogWarning("SMS service is not enabled or not properly configured");
                return false;
            }

            try
            {
                var formattedPhones = phoneNumbers.Select(FormatPhoneNumber).ToList();
                var successCount = 0;

                foreach (var phone in formattedPhones)
                {
                    var success = await SendSmsAsync(phone, message);
                    if (success) successCount++;
                    
                    // Add delay between SMS to avoid rate limiting
                    await Task.Delay(100);
                }

                _logger.LogInformation("Bulk SMS sent: {SuccessCount}/{TotalCount} successful", successCount, phoneNumbers.Count);
                return successCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk SMS to {Count} recipients", phoneNumbers.Count);
                return false;
            }
        }

        public async Task<bool> SendNoticeNotificationAsync(string noticeTitle, string noticeContent, List<string> memberPhoneNumbers)
        {
            if (!await IsSmsEnabledAsync())
            {
                _logger.LogWarning("SMS service is not enabled or not properly configured");
                return false;
            }

            try
            {
                // Create a concise SMS message for notice notification
                var message = CreateNoticeSmsMessage(noticeTitle, noticeContent);
                
                _logger.LogInformation("Sending notice notification SMS to {Count} members", memberPhoneNumbers.Count);
                
                return await SendBulkSmsAsync(memberPhoneNumbers, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notice notification SMS");
                return false;
            }
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters first
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Handle different formats
            if (digits.StartsWith("880"))
            {
                return digits; // Already in 880XXXXXXXXX format
            }
            else if (digits.StartsWith("88"))
            {
                return digits; // Already in 88XXXXXXXXX format
            }
            else if (digits.StartsWith("0"))
            {
                return "88" + digits.Substring(1); // Convert 017XXXXXXXX to 88017XXXXXXXX
            }
            else if (digits.Length == 11 && digits.StartsWith("1"))
            {
                return "88" + digits; // Convert 017XXXXXXXX to 88017XXXXXXXX
            }
            else
            {
                return "88" + digits; // Add 88 prefix
            }
        }

        private string CreateNoticeSmsMessage(string title, string content)
        {
            // Create a concise SMS message (max 160 characters)
            var message = $"নোটিশ: {title}";
            
            if (content.Length > 100)
            {
                message += $"\n{content.Substring(0, 100)}...";
            }
            else
            {
                message += $"\n{content}";
            }
            
            message += "\nসেক্টর ১৩ ওয়েলফেয়ার সোসাইটি";
            
            // Ensure message doesn't exceed SMS limit
            if (message.Length > 160)
            {
                message = message.Substring(0, 157) + "...";
            }
            
            return message;
        }

        private async Task<bool> SendViaBulkSmsBD(string phoneNumber, string message)
        {
            try
            {
                // Format phone number properly for BulkSmsBD
                var formattedPhone = phoneNumber.Replace("+", "");
                if (!formattedPhone.StartsWith("88"))
                {
                    formattedPhone = "88" + formattedPhone;
                }

                _logger.LogInformation("Original phone: {Original}, Formatted phone: {Formatted}", phoneNumber, formattedPhone);

                var queryParams = new List<string>
                {
                    $"api_key={Uri.EscapeDataString(_smsSettings.ApiKey)}",
                    $"type=text",
                    $"number={Uri.EscapeDataString(formattedPhone)}",
                    $"senderid={Uri.EscapeDataString(_smsSettings.SenderId)}",
                    $"message={Uri.EscapeDataString(message)}"
                };

                var apiUrl = $"{_smsSettings.ApiUrl}?{string.Join("&", queryParams)}";
                
                _logger.LogInformation("Sending SMS to BulkSmsBD API: {ApiUrl}", apiUrl);
                _logger.LogInformation("Phone: {Phone}, Message: {Message}", formattedPhone, message);

                var response = await _httpClient.GetAsync(apiUrl);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("BulkSmsBD Response Status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("BulkSmsBD Response Content: {Response}", responseContent);

                // Check for various success indicators
                var isSuccess = response.IsSuccessStatusCode &&
                              (responseContent.Contains("success") ||
                               responseContent.Contains("100") ||
                               responseContent.Contains("SMS sent") ||
                               responseContent.Contains("202"));

                if (isSuccess)
                {
                    _logger.LogInformation("SMS sent successfully to {Phone}", formattedPhone);
                }
                else
                {
                    _logger.LogWarning("SMS send failed to {Phone}. Response: {Response}", formattedPhone, responseContent);
                }

                return isSuccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS via BulkSmsBD to {Phone}", phoneNumber);
                return false;
            }
        }

        private async Task<bool> SendViaSslWireless(string phoneNumber, string message)
        {
            try
            {
                var requestData = new
                {
                    api_token = _smsSettings.ApiKey,
                    sid = _smsSettings.SenderId,
                    msisdn = phoneNumber,
                    sms = message,
                    csms_id = Guid.NewGuid().ToString()
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_smsSettings.ApiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("SSL Wireless Response: {Response}", responseContent);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS via SSL Wireless");
                return false;
            }
        }

        private async Task<bool> SendViaBulkSmsBDAlternative(string phoneNumber, string message)
        {
            try
            {
                // Alternative API format for BulkSmsBD
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("api_key", _smsSettings.ApiKey),
                    new KeyValuePair<string, string>("api_secret", _smsSettings.ApiSecret),
                    new KeyValuePair<string, string>("to", phoneNumber),
                    new KeyValuePair<string, string>("from", _smsSettings.SenderId),
                    new KeyValuePair<string, string>("message", message)
                });

                var response = await _httpClient.PostAsync(_smsSettings.ApiUrl, formData);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("BulkSmsBD Alternative Response: {Response}", responseContent);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS via BulkSmsBD Alternative");
                return false;
            }
        }
    }
} 