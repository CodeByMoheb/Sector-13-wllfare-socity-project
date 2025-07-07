using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;


namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class DonationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessDonation(Donor donor)
        {
            if (ModelState.IsValid)
            {
                // Generate receipt number
                donor.ReceiptNumber = GenerateReceiptNumber();
                donor.DonationDate = DateTime.Now;
                donor.PaymentStatus = "Pending";

                if (donor.PaymentMethod == "Manual")
                {
                    // For manual payment, mark as pending and show instructions
                    _context.Donors.Add(donor);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Thank you for your donation! Please complete the manual payment process.";
                    return RedirectToAction("ManualPaymentInstructions", new { id = donor.Id });
                }
                else if (donor.PaymentMethod == "SSLCommerz")
                {
                    // For SSL Commerz, save donor info and redirect to payment gateway
                    _context.Donors.Add(donor);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction("SSLCommerzPayment", new { id = donor.Id });
                }
            }

            return View("Index", donor);
        }

        public async Task<IActionResult> ManualPaymentInstructions(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null)
            {
                return NotFound();
            }

            return View(donor);
        }

        public async Task<IActionResult> SSLCommerzPayment(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null)
            {
                return NotFound();
            }

            // SSL Commerz configuration from appsettings
            var storeId = _configuration["SSLCommerz:StoreId"] ?? "";
            var storePassword = _configuration["SSLCommerz:StorePassword"] ?? "";
            var sessionApiUrl = _configuration["SSLCommerz:SessionApiUrl"] ?? "";
            var isSandbox = _configuration["SSLCommerz:IsSandbox"] == "true";

            // Validate configuration
            if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(storePassword) || string.IsNullOrEmpty(sessionApiUrl))
            {
                TempData["Error"] = "Payment gateway configuration is incomplete. Please contact support.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Generate unique transaction ID
            var tranId = $"TXN_{DateTime.Now:yyyyMMddHHmmss}_{donor.Id}";
            
            // Update donor with transaction ID
            donor.TransactionId = tranId;
            await _context.SaveChangesAsync();

            // Create payment request data
            var paymentData = new Dictionary<string, string>
            {
                ["store_id"] = storeId,
                ["store_passwd"] = storePassword,
                ["total_amount"] = donor.Amount.ToString("F2"),
                ["currency"] = "BDT",
                ["tran_id"] = tranId,
                ["product_category"] = "Donation",
                ["cus_name"] = donor.Name,
                ["cus_email"] = donor.Email,
                ["cus_add1"] = donor.Address ?? "Dhaka",
                ["cus_phone"] = donor.Phone,
                ["ship_name"] = donor.Name,
                ["ship_add1"] = donor.Address ?? "Dhaka",
                ["ship_city"] = "Dhaka",
                ["ship_postcode"] = "1000",
                ["ship_country"] = "Bangladesh",

                ["multi_card_name"] = "",
                ["value_a"] = donor.Id.ToString(),
                ["value_b"] = donor.DonationType ?? "General",
                ["value_c"] = donor.Message ?? ""
            };

            try
            {
                // Call SSL Commerz API to create session
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(paymentData);
                    var response = await client.PostAsync(sessionApiUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response
                    if (response.IsSuccessStatusCode)
                    {
                        var sslResponse = JsonSerializer.Deserialize<SSLCommerzResponse>(responseContent);
                        
                        if (sslResponse != null && sslResponse.Status == "SUCCESS" && !string.IsNullOrEmpty(sslResponse.GatewayPageURL))
                        {
                            // Redirect to the SSL Commerz gateway
                            return Redirect(sslResponse.GatewayPageURL);
                        }
                        else if (sslResponse != null && !string.IsNullOrEmpty(sslResponse.FailedReason))
                        {
                            TempData["Error"] = $"Payment gateway error: {sslResponse.FailedReason}";
                            return RedirectToAction("Index", "Home", new { area = "" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception

                TempData["Error"] = "Unable to connect to payment gateway. Please try again.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Fallback - show the payment form
            ViewBag.PaymentData = paymentData;
            ViewBag.SessionApiUrl = sessionApiUrl;
            ViewBag.IsSandbox = isSandbox;
            return View(donor);
        }



        [HttpPost]
        public async Task<IActionResult> PaymentFail()
        {
            try
            {
                var tranId = Request.Form["tran_id"].ToString() ?? "";
                if (string.IsNullOrEmpty(tranId))
                {
                    TempData["Error"] = "Invalid transaction ID.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.TransactionId == tranId);
                
                if (donor != null)
                {
                    donor.PaymentStatus = "Failed";
                    await _context.SaveChangesAsync();
                    
                    TempData["Error"] = "Payment failed. Please try again.";
                }

                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {

                TempData["Error"] = "An error occurred while processing payment failure.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PaymentCancel()
        {
            try
            {
                var tranId = Request.Form["tran_id"].ToString() ?? "";
                if (string.IsNullOrEmpty(tranId))
                {
                    TempData["Warning"] = "Invalid transaction ID.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.TransactionId == tranId);
                
                if (donor != null)
                {
                    donor.PaymentStatus = "Cancelled";
                    await _context.SaveChangesAsync();
                    
                    TempData["Warning"] = "Payment was cancelled.";
                }

                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {

                TempData["Error"] = "An error occurred while processing payment cancellation.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> IPN()
        {
            try
            {
                var tranId = Request.Form["tran_id"].ToString() ?? "";
                if (string.IsNullOrEmpty(tranId))
                {
                    return BadRequest("Invalid transaction ID");
                }

                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.TransactionId == tranId);
                
                if (donor != null)
                {
                    var status = Request.Form["status"].ToString() ?? "";
                    donor.PaymentStatus = status == "VALID" ? "Completed" : "Failed";
                    donor.TransactionId = Request.Form["bank_tran_id"].ToString() ?? tranId;
                    await _context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest();
            }
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> PaymentSuccess()
        {
            try
            {
                // Handle both GET and POST requests
                var tranId = Request.Form["tran_id"].ToString() ?? Request.Query["tran_id"].ToString() ?? "";
                if (string.IsNullOrEmpty(tranId))
                {
                    TempData["Error"] = "Invalid transaction ID.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                // Try to find donor by transaction ID first
                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.TransactionId == tranId);
                
                // If not found by transaction ID, try to find by the value_a field (donor ID)
                if (donor == null)
                {
                    var donorIdStr = Request.Form["value_a"].ToString() ?? Request.Query["value_a"].ToString();
                    if (!string.IsNullOrEmpty(donorIdStr) && int.TryParse(donorIdStr, out int donorId))
                    {
                        donor = await _context.Donors.FindAsync(donorId);
                    }
                }
                
                if (donor != null)
                {
                    // For sandbox testing, we'll skip validation and mark as successful
                    var isSandbox = _configuration["SSLCommerz:IsSandbox"] == "true";
                    
                    if (isSandbox)
                    {
                        // In sandbox mode, accept the payment without validation
                        donor.PaymentStatus = "Completed";
                        donor.TransactionId = Request.Form["bank_tran_id"].ToString() ?? Request.Query["bank_tran_id"].ToString() ?? tranId;
                        await _context.SaveChangesAsync();
                        
                        TempData["Success"] = "Payment successful! Thank you for your donation.";
                        return RedirectToAction("DonationSuccess", new { id = donor.Id });
                    }
                    else
                    {
                        // In production, validate the transaction with SSL Commerz
                        var isValid = await ValidateSSLCommerzTransaction(tranId);
                        
                        if (isValid)
                        {
                            donor.PaymentStatus = "Completed";
                            donor.TransactionId = Request.Form["bank_tran_id"].ToString() ?? Request.Query["bank_tran_id"].ToString() ?? tranId;
                            await _context.SaveChangesAsync();
                            
                            TempData["Success"] = "Payment successful! Thank you for your donation.";
                            return RedirectToAction("DonationSuccess", new { id = donor.Id });
                        }
                        else
                        {
                            donor.PaymentStatus = "Failed";
                            await _context.SaveChangesAsync();
                            
                            TempData["Error"] = "Payment validation failed. Please contact support.";
                            return RedirectToAction("Index", "Home", new { area = "" });
                        }
                    }
                }

                TempData["Error"] = "Donation not found.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                // Log the exception

                TempData["Error"] = "An error occurred while processing payment.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        public async Task<IActionResult> DonationSuccess(int id)
        {
            var donor = await _context.Donors.FindAsync(id);
            if (donor == null)
            {
                return NotFound();
            }

            return View(donor);
        }

        // Test method to directly access donation success page (for testing purposes)
        [HttpGet]
        public async Task<IActionResult> TestDonationSuccess()
        {
            // Get the most recent completed donation for testing
            var donor = await _context.Donors
                .Where(d => d.PaymentStatus == "Completed")
                .OrderByDescending(d => d.DonationDate)
                .FirstOrDefaultAsync();

            if (donor == null)
            {
                // Create a test donor if none exists
                donor = new Donor
                {
                    Name = "Test Donor",
                    Email = "test@example.com",
                    Phone = "01712345678",
                    Amount = 1000.00m,
                    PaymentMethod = "SSLCommerz",
                    TransactionId = "TEST_TXN_123",
                    PaymentStatus = "Completed",
                    DonationDate = DateTime.Now,
                    Message = "This is a test donation message.",
                    IsAnonymous = false,
                    DonationType = "General",
                    ReceiptNumber = "DON" + DateTime.Now.ToString("yyyyMMddHHmmss") + "1234"
                };

                _context.Donors.Add(donor);
                await _context.SaveChangesAsync();
            }

            return View("DonationSuccess", donor);
        }

        // Test page to access the test donation success
        [HttpGet]
        public IActionResult TestDonation()
        {
            return View();
        }

        public async Task<IActionResult> DonationHistory()
        {
            var donors = await _context.Donors
                .Where(d => d.PaymentStatus == "Completed")
                .OrderByDescending(d => d.DonationDate)
                .Take(20)
                .Select(d => new
                {
                    d.Name,
                    d.IsAnonymous,
                    d.Amount,
                    d.DonationType,
                    d.DonationDate,
                    d.Message
                })
                .ToListAsync();

            return Json(donors);
        }

        private string GenerateReceiptNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"DON{timestamp}{random}";
        }

        private async Task<bool> ValidateSSLCommerzTransaction(string tranId)
        {
            try
            {
                var storeId = _configuration["SSLCommerz:StoreId"] ?? "";
                var storePassword = _configuration["SSLCommerz:StorePassword"] ?? "";
                var validationApiUrl = _configuration["SSLCommerz:ValidationApiUrl"] ?? "";

                if (string.IsNullOrEmpty(storeId) || string.IsNullOrEmpty(storePassword) || string.IsNullOrEmpty(validationApiUrl))
                {
                    return false;
                }

                using (var client = new HttpClient())
                {
                    var validationData = new Dictionary<string, string>
                    {
                        ["store_id"] = storeId,
                        ["store_passwd"] = storePassword,
                        ["tran_id"] = tranId
                    };

                    var content = new FormUrlEncodedContent(validationData);
                    var response = await client.PostAsync(validationApiUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Parse the response to check if transaction is valid
                    return responseContent.Contains("VALID") || responseContent.Contains("success");
                }
            }
            catch
            {
                return false;
            }
        }
    }
} 