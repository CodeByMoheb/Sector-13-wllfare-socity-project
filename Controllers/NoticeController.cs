using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class NoticeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NoticeController> _logger;
        private readonly ISmsService _smsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NoticeController(
            ApplicationDbContext context, 
            ILogger<NoticeController> logger,
            ISmsService smsService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _smsService = smsService;
            _userManager = userManager;
        }

        // Test action for debugging
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Test database connection
                var noticeCount = await _context.Notices.CountAsync();
                
                // Test creating a sample notice
                var testNotice = new Notice
                {
                    Title = "Test Notice - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Content = "This is a test notice to verify database functionality.",
                    CreatedBy = User.Identity.Name ?? "TestUser",
                    CreatedAt = DateTime.Now,
                    IsApproved = false
                };
                
                _context.Notices.Add(testNotice);
                await _context.SaveChangesAsync();
                
                var result = new
                {
                    Success = true,
                    Message = "Database test successful",
                    NoticeCount = noticeCount,
                    TestNoticeId = testNotice.Id,
                    TestNoticeTitle = testNotice.Title
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database test failed");
                return Json(new { Success = false, Error = ex.Message });
            }
        }

        // Simple test action without authentication
        public async Task<IActionResult> TestCreateNotice()
        {
            try
            {
                var testNotice = new Notice
                {
                    Title = "Test Notice - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Content = "This is a test notice created without authentication.",
                    CreatedBy = "TestUser",
                    CreatedAt = DateTime.Now,
                    IsApproved = false
                };
                
                _context.Notices.Add(testNotice);
                await _context.SaveChangesAsync();
                
                return Json(new { 
                    Success = true, 
                    Message = "Test notice created successfully",
                    NoticeId = testNotice.Id,
                    Title = testNotice.Title
                });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        // Test SMS functionality
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestSms(string phoneNumber = null, string message = null)
        {
            try
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    phoneNumber = "+8801758051820"; // Updated to use +880 format
                }
                if (string.IsNullOrEmpty(message))
                {
                    message = "Test SMS from Sector 13 Welfare Society - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                // First check balance
                var balance = await _smsService.GetBalanceAsync();
                var isEnabled = await _smsService.IsSmsEnabledAsync();
                
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var configDebug = new
                {
                    ConfigIsEnabled = configuration["SmsSettings:IsEnabled"],
                    ConfigApiKey = configuration["SmsSettings:ApiKey"],
                    ConfigSenderId = configuration["SmsSettings:SenderId"],
                    ConfigApiUrl = configuration["SmsSettings:ApiUrl"]
                };

                if (!isEnabled)
                {
                    return Json(new {
                        Success = false,
                        Message = "SMS service is not enabled or not properly configured",
                        IsEnabled = false,
                        Balance = balance,
                        Debug = configDebug
                    });
                }

                // Check if balance is sufficient
                if (balance.Contains("Negative") || balance.Contains("0.00"))
                {
                    return Json(new {
                        Success = false,
                        Message = "Insufficient balance to send SMS",
                        IsEnabled = true,
                        Balance = balance,
                        PhoneNumber = phoneNumber,
                        MessageContent = message,
                        Debug = configDebug
                    });
                }

                // Send SMS with detailed logging
                _logger.LogInformation("Attempting to send SMS to {PhoneNumber} with message: {Message}", phoneNumber, message);
                
                var result = await _smsService.SendSmsAsync(phoneNumber, message);
                
                _logger.LogInformation("SMS send result: {Result} for phone {PhoneNumber}", result, phoneNumber);

                return Json(new {
                    Success = result,
                    Message = result ? "SMS sent successfully" : "Failed to send SMS",
                    PhoneNumber = phoneNumber,
                    MessageContent = message,
                    IsEnabled = true,
                    Balance = balance,
                    Debug = configDebug,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing SMS functionality");
                return Json(new {
                    Success = false,
                    Error = ex.Message,
                    IsEnabled = false,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Test bulk SMS to all members
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestBulkSms(string message = null)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "Test bulk SMS from Sector 13 Welfare Society - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                var isEnabled = await _smsService.IsSmsEnabledAsync();
                if (!isEnabled)
                {
                    return Json(new { 
                        Success = false, 
                        Message = "SMS service is not enabled or not properly configured",
                        IsEnabled = false
                    });
                }

                // Get all members with phone numbers
                var members = await _userManager.GetUsersInRoleAsync("Member");
                var memberPhones = members
                    .Where(u => !string.IsNullOrEmpty(u.PhoneNumber))
                    .Select(u => u.PhoneNumber)
                    .ToList();

                if (!memberPhones.Any())
                {
                    return Json(new { 
                        Success = false, 
                        Message = "No members with phone numbers found",
                        MemberCount = 0
                    });
                }

                var result = await _smsService.SendBulkSmsAsync(memberPhones, message);
                
                return Json(new { 
                    Success = result, 
                    Message = result ? "Bulk SMS sent successfully" : "Failed to send bulk SMS",
                    MemberCount = memberPhones.Count,
                    PhoneNumbers = memberPhones,
                    MessageContent = message,
                    IsEnabled = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing bulk SMS functionality");
                return Json(new { 
                    Success = false, 
                    Error = ex.Message, 
                    IsEnabled = false
                });
            }
        }

        // Check SMS balance
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckSmsBalance()
        {
            try
            {
                var balance = await _smsService.GetBalanceAsync();
                return Json(new { 
                    Success = true, 
                    Balance = balance
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SMS balance");
                return Json(new { 
                    Success = false, 
                    Error = ex.Message
                });
            }
        }

        // Debug SMS settings
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DebugSmsSettings()
        {
            try
            {
                // Get configuration directly
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var smsSettings = new
                {
                    IsEnabled = configuration["SmsSettings:IsEnabled"],
                    ApiKey = configuration["SmsSettings:ApiKey"],
                    SenderId = configuration["SmsSettings:SenderId"],
                    ApiUrl = configuration["SmsSettings:ApiUrl"],
                    Provider = configuration["SmsSettings:Provider"]
                };
                
                var isEnabled = await _smsService.IsSmsEnabledAsync();
                
                return Json(new { 
                    Success = true, 
                    Configuration = smsSettings,
                    ServiceEnabled = isEnabled
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debugging SMS settings");
                return Json(new { 
                    Success = false, 
                    Error = ex.Message
                });
            }
        }

        // Simple configuration test
        [Authorize(Roles = "Admin")]
        public IActionResult TestConfig()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var smsSettings = new
                {
                    IsEnabled = configuration["SmsSettings:IsEnabled"],
                    ApiKey = configuration["SmsSettings:ApiKey"],
                    SenderId = configuration["SmsSettings:SenderId"],
                    ApiUrl = configuration["SmsSettings:ApiUrl"],
                    Provider = configuration["SmsSettings:Provider"]
                };
                
                return Json(new { 
                    Success = true, 
                    Settings = smsSettings,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    Success = false, 
                    Error = ex.Message
                });
            }
        }

        // Direct balance check (no authentication required for testing)
        public async Task<IActionResult> BalanceCheck()
        {
            try
            {
                var balance = await _smsService.GetBalanceAsync();
                return Json(new { 
                    Success = true, 
                    Balance = balance,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    Success = false, 
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Test action to verify ManageMembers is accessible
        [Authorize(Roles = "Admin")]
        public IActionResult TestManageMembers()
        {
            return Json(new { 
                Success = true, 
                Message = "ManageMembers action is accessible",
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        // Manage member phone numbers
        [Authorize(Roles = "Admin")]
        [Route("Notice/ManageMembers")]
        [Route("Notice/ManageMembers/Index")]
        public async Task<IActionResult> ManageMembers()
        {
            try
            {
                var members = await _userManager.GetUsersInRoleAsync("Member");
                var memberList = members.Select(m => new
                {
                    Id = m.Id,
                    Name = m.Name ?? m.UserName ?? "Unknown",
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    HasPhone = !string.IsNullOrEmpty(m.PhoneNumber)
                }).ToList();

                _logger.LogInformation("Loaded {Count} members for management", memberList.Count);
                return View(memberList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading members for management");
                TempData["Error"] = "Error loading members: " + ex.Message;
                return View(new List<object>());
            }
        }

        // Update member phone number
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMemberPhone(string userId, string phoneNumber)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Json(new { Success = false, Message = "User not found" });
                }

                user.PhoneNumber = phoneNumber;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Json(new { Success = true, Message = "Phone number updated successfully" });
                }
                else
                {
                    return Json(new { Success = false, Message = "Failed to update phone number" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating member phone number");
                return Json(new { Success = false, Message = "Error updating phone number" });
            }
        }

        // Get member phone numbers for bulk SMS
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMemberPhones()
        {
            try
            {
                var members = await _userManager.GetUsersInRoleAsync("Member");
                var memberPhones = members
                    .Where(u => !string.IsNullOrEmpty(u.PhoneNumber))
                    .Select(u => new
                    {
                        Id = u.Id,
                        Name = u.Name,
                        PhoneNumber = u.PhoneNumber
                    })
                    .ToList();

                return Json(new { 
                    Success = true, 
                    Members = memberPhones,
                    Count = memberPhones.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member phone numbers");
                return Json(new { Success = false, Error = ex.Message });
            }
        }

        // GET: /Notice/ManagerList
        [Authorize(Roles = "Manager")]
        public IActionResult ManagerList()
        {
            try
            {
                var notices = _context.Notices
                    .Where(n => n.CreatedBy == User.Identity.Name)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();
                return View(notices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading manager notices for user: {UserName}", User.Identity.Name);
                TempData["Error"] = "Error loading notices. Please try again.";
                return View(new List<Notice>());
            }
        }

        // GET: /Notice/Create
        [Authorize(Roles = "Manager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Notice/Create
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notice notice)
        {
            _logger.LogInformation("Notice creation attempt by user: {UserName}", User.Identity.Name);
            
            try
            {
                // Set the CreatedBy field before validation
                notice.CreatedBy = User.Identity.Name ?? "Unknown";
                notice.CreatedAt = DateTime.Now;
                notice.IsApproved = false;
                notice.ApprovedBy = null;
                notice.ApprovedAt = null;
                
                // Validate required fields manually
                if (string.IsNullOrWhiteSpace(notice.Title))
                {
                    TempData["Error"] = "Notice title is required.";
                    return View(notice);
                }
                
                if (string.IsNullOrWhiteSpace(notice.Content))
                {
                    TempData["Error"] = "Notice content is required.";
                    return View(notice);
                }
                
                // Clear any model state errors for CreatedBy since we set it manually
                ModelState.Remove("CreatedBy");
                
                // Check if model is valid after our manual validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));
                    
                    foreach (var error in errors)
                    {
                        TempData["Error"] = error;
                    }
                    return View(notice);
                }
                
                _logger.LogInformation("Adding notice to database: Title={Title}, CreatedBy={CreatedBy}", 
                    notice.Title, notice.CreatedBy);
                
                _context.Notices.Add(notice);
                var result = await _context.SaveChangesAsync();
                
                _logger.LogInformation("Notice saved successfully. ID: {NoticeId}, Rows affected: {RowsAffected}", 
                    notice.Id, result);
                
                TempData["Success"] = "Notice created successfully and sent for approval.";
                return RedirectToAction("ManagerList");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notice for user: {UserName}", User.Identity.Name);
                ModelState.AddModelError("", "An error occurred while creating the notice. Please try again.");
                TempData["Error"] = "An error occurred while creating the notice. Please try again.";
                return View(notice);
            }
        }

        // GET: /Notice/SecretaryList
        [Authorize(Roles = "Secretary")]
        public IActionResult SecretaryList()
        {
            try
            {
                var notices = _context.Notices
                    .Where(n => !n.IsApproved)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();
                return View(notices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading secretary notices");
                TempData["Error"] = "Error loading pending notices. Please try again.";
                return View(new List<Notice>());
            }
        }

        // POST: /Notice/Approve/{id}
        [HttpPost]
        [Authorize(Roles = "Secretary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var notice = await _context.Notices.FirstOrDefaultAsync(n => n.Id == id);
                if (notice != null && !notice.IsApproved)
                {
                    notice.IsApproved = true;
                    notice.ApprovedBy = User.Identity.Name;
                    notice.ApprovedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Notice {NoticeId} approved by {ApprovedBy}", id, User.Identity.Name);
                    TempData["Success"] = "Notice approved successfully.";
                    
                    // Send SMS notifications to all members
                    await SendNoticeSmsNotifications(notice);
                }
                else
                {
                    _logger.LogWarning("Notice {NoticeId} not found or already approved", id);
                    TempData["Error"] = "Notice not found or already approved.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving notice {NoticeId}", id);
                TempData["Error"] = "An error occurred while approving the notice.";
            }
            
            return RedirectToAction("SecretaryList");
        }

        // Private method to send SMS notifications for approved notices
        private async Task SendNoticeSmsNotifications(Notice notice)
        {
            try
            {
                _logger.LogInformation("Starting SMS notifications for notice: {NoticeTitle}", notice.Title);

                if (!await _smsService.IsSmsEnabledAsync())
                {
                    _logger.LogInformation("SMS service is not enabled. Skipping SMS notifications.");
                    return;
                }

                // Get sheet members with phone numbers
                var sheetMembers = await _context.SheetMembers
                    .Where(m => !string.IsNullOrEmpty(m.PhoneNumber))
                    .ToListAsync();

                _logger.LogInformation("Found {Count} sheet members with phone numbers", sheetMembers.Count);

                if (!sheetMembers.Any())
                {
                    _logger.LogWarning("No sheet members with phone numbers found for SMS notification");
                    return;
                }

                var memberPhones = sheetMembers.Select(m => m.PhoneNumber).ToList();

                _logger.LogInformation("Sending SMS notifications to {Count} sheet members for notice: {NoticeTitle}",
                    memberPhones.Count, notice.Title);

                // Create notice message
                var noticeMessage = $"নোটিশ: {notice.Title} - {notice.Content} - সেক্টর ১৩ ওয়েলফেয়ার সোসাইটি";
                
                _logger.LogInformation("Notice message: {Message}", noticeMessage);

                // Send SMS using bulk SMS method
                var smsResult = await _smsService.SendBulkSmsAsync(memberPhones, noticeMessage);

                if (smsResult)
                {
                    _logger.LogInformation("SMS notifications sent successfully to sheet members for notice: {NoticeTitle}", notice.Title);
                    TempData["SmsSuccess"] = $"SMS notifications sent to {memberPhones.Count} sheet members.";
                }
                else
                {
                    _logger.LogWarning("Failed to send SMS notifications to sheet members for notice: {NoticeTitle}", notice.Title);
                    TempData["SmsWarning"] = "Notice approved but SMS notifications failed to send to sheet members.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS notifications to sheet members for notice: {NoticeTitle}", notice.Title);
                TempData["SmsError"] = $"Error sending SMS: {ex.Message}";
            }
        }

        // GET: /Notice/PublicList - Public notice listing
        public async Task<IActionResult> PublicList()
        {
            try
            {
                var approvedNotices = await _context.Notices
                    .Where(n => n.IsApproved)
                    .OrderByDescending(n => n.ApprovedAt)
                    .ToListAsync();
                
                return View(approvedNotices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public notices");
                return View(new List<Notice>());
            }
        }

        // GET: /Notice/Details/{id} - Public notice details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var notice = await _context.Notices
                    .FirstOrDefaultAsync(n => n.Id == id && n.IsApproved);
                
                if (notice == null)
                {
                    return NotFound();
                }
                
                return View(notice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notice details for ID: {NoticeId}", id);
                return NotFound();
            }
        }

        // GET: /Notice/AllNotices - For admin to see all notices
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllNotices()
        {
            try
            {
                var allNotices = await _context.Notices
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
                
                return View(allNotices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading all notices for admin");
                TempData["Error"] = "Error loading notices. Please try again.";
                return View(new List<Notice>());
            }
        }

        // Direct API test for debugging
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestDirectApi()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var apiKey = configuration["SmsSettings:ApiKey"];
                var senderId = configuration["SmsSettings:SenderId"];
                var apiUrl = configuration["SmsSettings:ApiUrl"];

                // Test phone number
                var testPhone = "+8801758051820"; // Your number in +880 format
                var testMessage = "Direct API Test with Sender ID " + senderId + " - " + DateTime.Now.ToString("HH:mm:ss");

                var queryParams = new List<string>
                {
                    $"api_key={Uri.EscapeDataString(apiKey)}",
                    $"type=text",
                    $"number={Uri.EscapeDataString(testPhone)}",
                    $"senderid={Uri.EscapeDataString(senderId)}",
                    $"message={Uri.EscapeDataString(testMessage)}"
                };

                var fullUrl = $"{apiUrl}?{string.Join("&", queryParams)}";

                _logger.LogInformation("Testing direct API call with sender ID {SenderId}: {Url}", senderId, fullUrl);

                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(fullUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                return Json(new
                {
                    Success = response.IsSuccessStatusCode,
                    ApiUrl = fullUrl,
                    ResponseStatus = response.StatusCode,
                    ResponseContent = responseContent,
                    TestPhone = testPhone,
                    TestMessage = testMessage,
                    SenderId = senderId,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in direct API test");
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Detailed API test to debug SMS delivery
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DebugSmsDelivery()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var apiKey = configuration["SmsSettings:ApiKey"];
                var senderId = configuration["SmsSettings:SenderId"];
                var apiUrl = configuration["SmsSettings:ApiUrl"];

                // Test phone number
                var testPhone = "+8801758051820";
                var testMessage = "Debug Test - " + DateTime.Now.ToString("HH:mm:ss");

                var queryParams = new List<string>
                {
                    $"api_key={Uri.EscapeDataString(apiKey)}",
                    $"type=text",
                    $"number={Uri.EscapeDataString(testPhone)}",
                    $"senderid={Uri.EscapeDataString(senderId)}",
                    $"message={Uri.EscapeDataString(testMessage)}"
                };

                var fullUrl = $"{apiUrl}?{string.Join("&", queryParams)}";

                _logger.LogInformation("Debug SMS API call: {Url}", fullUrl);

                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var response = await httpClient.GetAsync(fullUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Parse response to understand what BulkSmsBD is saying
                var responseAnalysis = new
                {
                    IsSuccessStatusCode = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    RawResponse = responseContent,
                    ContainsSuccess = responseContent.Contains("success", StringComparison.OrdinalIgnoreCase),
                    ContainsError = responseContent.Contains("error", StringComparison.OrdinalIgnoreCase),
                    ContainsInvalid = responseContent.Contains("invalid", StringComparison.OrdinalIgnoreCase),
                    ContainsBalance = responseContent.Contains("balance", StringComparison.OrdinalIgnoreCase),
                    ContainsSender = responseContent.Contains("sender", StringComparison.OrdinalIgnoreCase)
                };

                return Json(new
                {
                    Success = response.IsSuccessStatusCode,
                    ApiUrl = fullUrl,
                    ResponseAnalysis = responseAnalysis,
                    TestPhone = testPhone,
                    TestMessage = testMessage,
                    SenderId = senderId,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in debug SMS delivery");
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    StackTrace = ex.StackTrace,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Test sender ID validation
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestSenderId()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var apiKey = configuration["SmsSettings:ApiKey"];
                var senderId = configuration["SmsSettings:SenderId"];

                // Test with a minimal message to check sender ID
                var testPhone = "+8801758051820";
                var testMessage = "Test";

                var queryParams = new List<string>
                {
                    $"api_key={Uri.EscapeDataString(apiKey)}",
                    $"type=text",
                    $"number={Uri.EscapeDataString(testPhone)}",
                    $"senderid={Uri.EscapeDataString(senderId)}",
                    $"message={Uri.EscapeDataString(testMessage)}"
                };

                var apiUrl = configuration["SmsSettings:ApiUrl"];
                var fullUrl = $"{apiUrl}?{string.Join("&", queryParams)}";

                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(fullUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                return Json(new
                {
                    Success = response.IsSuccessStatusCode,
                    SenderId = senderId,
                    ApiKey = apiKey,
                    Response = responseContent,
                    StatusCode = response.StatusCode,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Test with alternative sender ID
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestAlternativeSender()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var apiKey = configuration["SmsSettings:ApiKey"];
                var originalSenderId = configuration["SmsSettings:SenderId"];
                
                // Try with alternative sender IDs
                var alternativeSenderIds = new[] { "INFO", "ALERT", "NOTICE", "SMS" };
                var results = new List<object>();

                foreach (var senderId in alternativeSenderIds)
                {
                    var testPhone = "+8801758051820";
                    var testMessage = "Test with " + senderId + " - " + DateTime.Now.ToString("HH:mm:ss");

                    var queryParams = new List<string>
                    {
                        $"api_key={Uri.EscapeDataString(apiKey)}",
                        $"type=text",
                        $"number={Uri.EscapeDataString(testPhone)}",
                        $"senderid={Uri.EscapeDataString(senderId)}",
                        $"message={Uri.EscapeDataString(testMessage)}"
                    };

                    var apiUrl = configuration["SmsSettings:ApiUrl"];
                    var fullUrl = $"{apiUrl}?{string.Join("&", queryParams)}";

                    var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(fullUrl);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    results.Add(new
                    {
                        SenderId = senderId,
                        Success = response.IsSuccessStatusCode,
                        Response = responseContent,
                        StatusCode = response.StatusCode
                    });
                }

                return Json(new
                {
                    OriginalSenderId = originalSenderId,
                    AlternativeTests = results,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Comprehensive API test to compare with dashboard
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CompareWithDashboard()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var apiKey = configuration["SmsSettings:ApiKey"];
                var senderId = configuration["SmsSettings:SenderId"];
                var apiUrl = configuration["SmsSettings:ApiUrl"];

                // Test phone number
                var testPhone = "+8801758051820";
                var testMessage = "System Test - " + DateTime.Now.ToString("HH:mm:ss");

                // Format phone number exactly like dashboard
                var formattedPhone = testPhone.Replace("+", "");
                if (!formattedPhone.StartsWith("88"))
                {
                    formattedPhone = "88" + formattedPhone;
                }

                var queryParams = new List<string>
                {
                    $"api_key={Uri.EscapeDataString(apiKey)}",
                    $"type=text",
                    $"number={Uri.EscapeDataString(formattedPhone)}",
                    $"senderid={Uri.EscapeDataString(senderId)}",
                    $"message={Uri.EscapeDataString(testMessage)}"
                };

                var fullUrl = $"{apiUrl}?{string.Join("&", queryParams)}";

                _logger.LogInformation("Testing API call: {Url}", fullUrl);

                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await httpClient.GetAsync(fullUrl);
                stopwatch.Stop();
                
                var responseContent = await response.Content.ReadAsStringAsync();

                // Detailed analysis
                var analysis = new
                {
                    RequestUrl = fullUrl,
                    RequestMethod = "GET",
                    ResponseTime = stopwatch.ElapsedMilliseconds + "ms",
                    StatusCode = response.StatusCode,
                    StatusCodeNumber = (int)response.StatusCode,
                    RawResponse = responseContent,
                    ResponseLength = responseContent.Length,
                    Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                    IsSuccessStatusCode = response.IsSuccessStatusCode,
                    ContainsSuccess = responseContent.Contains("success", StringComparison.OrdinalIgnoreCase),
                    ContainsError = responseContent.Contains("error", StringComparison.OrdinalIgnoreCase),
                    ContainsInvalid = responseContent.Contains("invalid", StringComparison.OrdinalIgnoreCase),
                    ContainsBalance = responseContent.Contains("balance", StringComparison.OrdinalIgnoreCase),
                    ContainsSender = responseContent.Contains("sender", StringComparison.OrdinalIgnoreCase),
                    ContainsApiKey = responseContent.Contains("api_key", StringComparison.OrdinalIgnoreCase),
                    ContainsNumber = responseContent.Contains("number", StringComparison.OrdinalIgnoreCase)
                };

                return Json(new
                {
                    Success = response.IsSuccessStatusCode,
                    Analysis = analysis,
                    TestPhone = testPhone,
                    FormattedPhone = formattedPhone,
                    TestMessage = testMessage,
                    SenderId = senderId,
                    ApiKey = apiKey,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in comprehensive API test");
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    ErrorType = ex.GetType().Name,
                    StackTrace = ex.StackTrace,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Test different API formats to match dashboard
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestDashboardFormat()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var apiKey = configuration["SmsSettings:ApiKey"];
                var senderId = configuration["SmsSettings:SenderId"];
                var apiUrl = configuration["SmsSettings:ApiUrl"];

                var testPhone = "+8801758051820";
                var testMessage = "Dashboard Format Test - " + DateTime.Now.ToString("HH:mm:ss");

                var results = new List<object>();

                // Test 1: With +880 format
                var phone1 = "8801758051820";
                var result1 = await TestSingleFormat(apiKey, senderId, apiUrl, phone1, testMessage, "Test 1: 8801758051820");
                results.Add(result1);

                // Test 2: Without 88 prefix
                var phone2 = "01758051820";
                var result2 = await TestSingleFormat(apiKey, senderId, apiUrl, phone2, testMessage, "Test 2: 01758051820");
                results.Add(result2);

                // Test 3: With different sender ID
                var result3 = await TestSingleFormat(apiKey, "INFO", apiUrl, phone1, testMessage, "Test 3: Sender ID INFO");
                results.Add(result3);

                // Test 4: With different message format
                var result4 = await TestSingleFormat(apiKey, senderId, apiUrl, phone1, "Test", "Test 4: Short message");
                results.Add(result4);

                return Json(new
                {
                    Success = true,
                    Tests = results,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        private async Task<object> TestSingleFormat(string apiKey, string senderId, string apiUrl, string phone, string message, string testName)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"api_key={Uri.EscapeDataString(apiKey)}",
                    $"type=text",
                    $"number={Uri.EscapeDataString(phone)}",
                    $"senderid={Uri.EscapeDataString(senderId)}",
                    $"message={Uri.EscapeDataString(message)}"
                };

                var fullUrl = $"{apiUrl}?{string.Join("&", queryParams)}";

                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(fullUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                return new
                {
                    TestName = testName,
                    Phone = phone,
                    SenderId = senderId,
                    Message = message,
                    Success = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode,
                    Response = responseContent,
                    Url = fullUrl
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    TestName = testName,
                    Phone = phone,
                    SenderId = senderId,
                    Message = message,
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        // Show current server IP for whitelisting
        [Authorize(Roles = "Admin")]
        public IActionResult ShowServerIp()
        {
            try
            {
                var serverIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var localIp = HttpContext.Connection.LocalIpAddress?.ToString();
                
                // Get external IP if possible
                var externalIp = "163.53.182.12"; // From your error message
                
                return Json(new
                {
                    Success = true,
                    ServerIP = externalIp,
                    LocalIP = localIp,
                    RemoteIP = serverIp,
                    Message = "Add this IP to BulkSmsBD whitelist: " + externalIp,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Import phone numbers from sheet
        [Authorize(Roles = "Admin")]
        public IActionResult ImportPhoneNumbers()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportPhoneNumbers(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    TempData["Error"] = "Please select a file to upload.";
                    return RedirectToAction("ImportPhoneNumbers");
                }

                // First, ensure the SheetMembers table exists
                try
                {
                    await _context.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SheetMembers' AND xtype='U')
                        CREATE TABLE SheetMembers (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(100) NOT NULL,
                            PhoneNumber NVARCHAR(20) NULL,
                            CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                            UpdatedAt DATETIME2 NULL
                        )");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating SheetMembers table");
                    TempData["Error"] = "Error creating database table: " + ex.Message;
                    return RedirectToAction("ImportPhoneNumbers");
                }

                var phoneNumbers = new List<SheetMember>();
                var extension = Path.GetExtension(file.FileName).ToLower();

                using (var stream = file.OpenReadStream())
                {
                    if (extension == ".csv")
                    {
                        phoneNumbers = await ParseCsvFile(stream);
                    }
                    else if (extension == ".xlsx" || extension == ".xls")
                    {
                        phoneNumbers = await ParseExcelFile(stream);
                    }
                    else
                    {
                        TempData["Error"] = "Please upload a CSV or Excel file.";
                        return RedirectToAction("ImportPhoneNumbers");
                    }
                }

                if (!phoneNumbers.Any())
                {
                    TempData["Error"] = "No valid phone numbers found in the file. Please check the format.";
                    return RedirectToAction("ImportPhoneNumbers");
                }

                // Save to database
                await SavePhoneNumbersToDatabase(phoneNumbers);

                TempData["Success"] = $"Successfully imported {phoneNumbers.Count} phone numbers from sheet.";
                return RedirectToAction("ManageSheetMembers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing phone numbers from sheet");
                TempData["Error"] = "Error importing file: " + ex.Message;
                return RedirectToAction("ImportPhoneNumbers");
            }
        }

        // Manage sheet-based members
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageSheetMembers()
        {
            try
            {
                var sheetMembers = await _context.SheetMembers.ToListAsync();
                return View(sheetMembers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading sheet members");
                TempData["Error"] = "Error loading members.";
                return View(new List<SheetMember>());
            }
        }

        // Update sheet member phone number
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSheetMemberPhone(int id, string phoneNumber)
        {
            try
            {
                var member = await _context.SheetMembers.FindAsync(id);
                if (member == null)
                {
                    return Json(new { Success = false, Message = "Member not found" });
                }

                member.PhoneNumber = phoneNumber;
                member.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { Success = true, Message = "Phone number updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sheet member phone number");
                return Json(new { Success = false, Message = "Error updating phone number" });
            }
        }

        // Delete sheet member
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSheetMember(int id)
        {
            try
            {
                var member = await _context.SheetMembers.FindAsync(id);
                if (member == null)
                {
                    return Json(new { Success = false, Message = "Member not found" });
                }

                _context.SheetMembers.Remove(member);
                await _context.SaveChangesAsync();

                return Json(new { Success = true, Message = "Member deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sheet member");
                return Json(new { Success = false, Message = "Error deleting member" });
            }
        }

        // Get sheet member phone numbers for bulk SMS
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSheetMemberPhones()
        {
            try
            {
                var members = await _context.SheetMembers
                    .Where(m => !string.IsNullOrEmpty(m.PhoneNumber))
                    .Select(m => new
                    {
                        Id = m.Id,
                        Name = m.Name,
                        PhoneNumber = m.PhoneNumber
                    })
                    .ToListAsync();

                return Json(new { 
                    Success = true, 
                    Members = members,
                    Count = members.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sheet member phone numbers");
                return Json(new { Success = false, Error = ex.Message });
            }
        }

        // Test bulk SMS to sheet members
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestBulkSmsToSheetMembers(string message = null)
        {
            try
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "Test SMS to Sheet Members - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                var members = await _context.SheetMembers
                    .Where(m => !string.IsNullOrEmpty(m.PhoneNumber))
                    .ToListAsync();

                if (!members.Any())
                {
                    return Json(new { 
                        Success = false, 
                        Message = "No members with phone numbers found" 
                    });
                }

                var phoneNumbers = members.Select(m => m.PhoneNumber).ToList();
                var result = await _smsService.SendBulkSmsAsync(phoneNumbers, message);

                return Json(new {
                    Success = result,
                    Message = result ? "Bulk SMS sent successfully" : "Failed to send bulk SMS",
                    PhoneNumbers = phoneNumbers,
                    Count = phoneNumbers.Count,
                    MessageContent = message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing bulk SMS to sheet members");
                return Json(new {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Create SheetMembers table if it doesn't exist
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSheetMembersTable()
        {
            try
            {
                // Check if table exists
                var tableExists = await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SheetMembers' AND xtype='U')
                    CREATE TABLE SheetMembers (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        PhoneNumber NVARCHAR(20) NULL,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                        UpdatedAt DATETIME2 NULL
                    )");

                return Json(new { 
                    Success = true, 
                    Message = "SheetMembers table created successfully or already exists",
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SheetMembers table");
                return Json(new { 
                    Success = false, 
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Check if SheetMembers table exists
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CheckSheetMembersTable()
        {
            try
            {
                var tableExists = await _context.Database.ExecuteSqlRawAsync(@"
                    SELECT COUNT(*) FROM sysobjects WHERE name='SheetMembers' AND xtype='U'");
                
                var exists = tableExists > 0;
                
                return Json(new { 
                    Success = true, 
                    TableExists = exists,
                    Message = exists ? "SheetMembers table exists" : "SheetMembers table does not exist",
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking SheetMembers table");
                return Json(new { 
                    Success = false, 
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        private async Task<List<SheetMember>> ParseCsvFile(Stream stream)
        {
            var phoneNumbers = new List<SheetMember>();
            using (var reader = new StreamReader(stream))
            {
                string line;
                bool isFirstLine = true;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue; // Skip header
                    }

                    var columns = line.Split(',');
                    if (columns.Length >= 2)
                    {
                        var name = columns[0].Trim().Trim('"');
                        var phone = columns[1].Trim().Trim('"');
                        
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(phone))
                        {
                            phoneNumbers.Add(new SheetMember
                            {
                                Name = name,
                                PhoneNumber = phone,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }
                }
            }
            return phoneNumbers;
        }

        private async Task<List<SheetMember>> ParseExcelFile(Stream stream)
        {
            // For now, we'll use a simple approach
            // You might want to add EPPlus or similar library for Excel parsing
            var phoneNumbers = new List<SheetMember>();
            
            // Placeholder - you can implement Excel parsing here
            // For now, we'll return empty list and suggest CSV format
            return phoneNumbers;
        }

        private async Task SavePhoneNumbersToDatabase(List<SheetMember> phoneNumbers)
        {
            try
            {
                // Clear existing data (optional - you can modify this behavior)
                var existingMembers = await _context.SheetMembers.ToListAsync();
                if (existingMembers.Any())
                {
                    _context.SheetMembers.RemoveRange(existingMembers);
                }
                
                // Add new members
                await _context.SheetMembers.AddRangeAsync(phoneNumbers);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving phone numbers to database");
                throw new Exception("Failed to save phone numbers to database: " + ex.Message);
            }
        }

        // Test with pre-approved sender IDs
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestPreApprovedSenders()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetService<IConfiguration>();
                var apiKey = configuration["SmsSettings:ApiKey"];
                var apiUrl = configuration["SmsSettings:ApiUrl"];

                // Pre-approved sender IDs that might work
                var preApprovedSenders = new[] { "INFO", "ALERT", "NOTICE", "SMS", "DEMO" };
                var testPhone = "+8801758051820";
                var testMessage = "Test with pre-approved sender - " + DateTime.Now.ToString("HH:mm:ss");

                var results = new List<object>();

                foreach (var senderId in preApprovedSenders)
                {
                    var queryParams = new List<string>
                    {
                        $"api_key={Uri.EscapeDataString(apiKey)}",
                        $"type=text",
                        $"number={Uri.EscapeDataString("8801758051820")}",
                        $"senderid={Uri.EscapeDataString(senderId)}",
                        $"message={Uri.EscapeDataString(testMessage)}"
                    };

                    var fullUrl = $"{apiUrl}?{string.Join("&", queryParams)}";

                    var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(fullUrl);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    results.Add(new
                    {
                        SenderId = senderId,
                        Success = response.IsSuccessStatusCode,
                        StatusCode = response.StatusCode,
                        Response = responseContent,
                        ContainsError = responseContent.Contains("error", StringComparison.OrdinalIgnoreCase),
                        ContainsWhitelist = responseContent.Contains("whitelist", StringComparison.OrdinalIgnoreCase),
                        ContainsSuccess = responseContent.Contains("success", StringComparison.OrdinalIgnoreCase)
                    });
                }

                return Json(new
                {
                    Success = true,
                    TestPhone = testPhone,
                    TestMessage = testMessage,
                    Results = results,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Debug sheet member SMS configuration
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DebugSheetMemberSms()
        {
            try
            {
                // Get all sheet members
                var allSheetMembers = await _context.SheetMembers.ToListAsync();
                
                // Get sheet members with phone numbers
                var sheetMembersWithPhones = await _context.SheetMembers
                    .Where(m => !string.IsNullOrEmpty(m.PhoneNumber))
                    .ToListAsync();

                // Test SMS service
                var isSmsEnabled = await _smsService.IsSmsEnabledAsync();
                var balance = await _smsService.GetBalanceAsync();

                // Test with first member's phone number
                var testResult = false;
                var testPhone = "";
                var testMessage = "";
                
                if (sheetMembersWithPhones.Any())
                {
                    testPhone = sheetMembersWithPhones.First().PhoneNumber;
                    testMessage = "Debug Test - " + DateTime.Now.ToString("HH:mm:ss");
                    testResult = await _smsService.SendSmsAsync(testPhone, testMessage);
                }

                return Json(new
                {
                    Success = true,
                    AllSheetMembers = allSheetMembers.Count,
                    SheetMembersWithPhones = sheetMembersWithPhones.Count,
                    PhoneNumbers = sheetMembersWithPhones.Select(m => new { 
                        Id = m.Id, 
                        Name = m.Name, 
                        Phone = m.PhoneNumber 
                    }).ToList(),
                    SmsServiceEnabled = isSmsEnabled,
                    Balance = balance,
                    TestPhone = testPhone,
                    TestMessage = testMessage,
                    TestResult = testResult,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error debugging sheet member SMS");
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    StackTrace = ex.StackTrace,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }

        // Test notice approval SMS process
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestNoticeApprovalSms()
        {
            try
            {
                // Create a test notice
                var testNotice = new Notice
                {
                    Title = "Test Notice Approval",
                    Content = "This is a test notice to verify SMS functionality",
                    CreatedBy = "Test User",
                    CreatedAt = DateTime.Now,
                    IsApproved = true,
                    ApprovedBy = "Test Secretary",
                    ApprovedAt = DateTime.Now
                };

                _logger.LogInformation("Testing notice approval SMS process");

                // Call the same method that's called during notice approval
                await SendNoticeSmsNotifications(testNotice);

                return Json(new
                {
                    Success = true,
                    Message = "Notice approval SMS test completed. Check logs for details.",
                    TestNotice = new
                    {
                        Title = testNotice.Title,
                        Content = testNotice.Content
                    },
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing notice approval SMS");
                return Json(new
                {
                    Success = false,
                    Error = ex.Message,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
        }
    }
} 