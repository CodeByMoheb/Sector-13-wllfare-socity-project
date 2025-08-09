  using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Sector_13_Welfare_Society___Digital_Management_System.Models.Services.Sms;


namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class DonationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ISmsSender _smsSender;
        private readonly IEmailService _emailService;

        public DonationController(ApplicationDbContext context, IConfiguration configuration, ISmsSender smsSender, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _smsSender = smsSender;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> Report([FromQuery] DonationReportFilter filter)
        {
            var query = _context.Donors.AsQueryable();
            if (filter.StartDate.HasValue) query = query.Where(d => d.DonationDate >= filter.StartDate.Value);
            if (filter.EndDate.HasValue) query = query.Where(d => d.DonationDate <= filter.EndDate.Value);
            if (!string.IsNullOrEmpty(filter.PaymentMethod)) query = query.Where(d => d.PaymentMethod == filter.PaymentMethod);
            if (!string.IsNullOrEmpty(filter.Status)) query = query.Where(d => d.PaymentStatus == filter.Status);
            if (!string.IsNullOrEmpty(filter.DonationType)) query = query.Where(d => d.DonationType == filter.DonationType);
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var s = filter.Search;
                query = query.Where(d => d.Name.Contains(s) || d.Email.Contains(s) || d.Phone.Contains(s) || (d.TransactionId ?? "").Contains(s));
            }

            // Stats
            var completed = query.Where(d => d.PaymentStatus == "Completed");
            var vm = new DonationReportViewModel();
            vm.Filter = filter;
            vm.TotalAmount = await completed.SumAsync(d => (decimal?)d.Amount) ?? 0m;
            vm.TotalCount = await completed.CountAsync();

            var today = DateTime.Today;
            vm.TodayAmount = await completed.Where(d => d.DonationDate >= today).SumAsync(d => (decimal?)d.Amount) ?? 0m;

            var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Sunday);
            vm.ThisWeekAmount = await completed.Where(d => d.DonationDate >= weekStart).SumAsync(d => (decimal?)d.Amount) ?? 0m;

            var monthStart = new DateTime(today.Year, today.Month, 1);
            vm.ThisMonthAmount = await completed.Where(d => d.DonationDate >= monthStart).SumAsync(d => (decimal?)d.Amount) ?? 0m;

            vm.AveragePerDonation = vm.TotalCount > 0 ? Math.Round(vm.TotalAmount / vm.TotalCount, 2) : 0m;

            // Top donors (by total amount)
            vm.TopDonors = await completed
                .GroupBy(d => new { d.Name, d.Email, d.Phone })
                .Select(g => new TopDonorItem
                {
                    Name = g.Key.Name,
                    Email = g.Key.Email,
                    Phone = g.Key.Phone,
                    TotalAmount = g.Sum(x => x.Amount),
                    Donations = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(10)
                .ToListAsync();

            // Available filters
            vm.AvailablePaymentMethods = await _context.Donors.Select(d => d.PaymentMethod).Distinct().ToListAsync();

            // Paged donations table (desc by date)
            var pageSize = 20;
            var ordered = query.OrderByDescending(d => d.DonationDate);
            vm.Donations = await PaginatedList<Donor>.CreateAsync(ordered.AsNoTracking(), filter.PageNumber <= 0 ? 1 : filter.PageNumber, pageSize);

            return View(vm);
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
            var storeName = _configuration["SSLCommerz:StoreName"] ?? "Sector 13 Welfare Society";
            var registeredUrl = _configuration["SSLCommerz:RegisteredUrl"] ?? "www.wfs-13.org";

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

            // Create payment request data with correct SSL Commerz parameters
            var paymentData = new Dictionary<string, string>
            {
                ["store_id"] = storeId,
                ["store_passwd"] = storePassword,
                ["total_amount"] = donor.Amount.ToString("F2"),
                ["currency"] = "BDT",
                ["tran_id"] = tranId,
                ["product_category"] = "Donation",
                ["product_name"] = $"Donation - {donor.DonationType ?? "General"}",
                ["product_profile"] = "general",
                ["cus_name"] = donor.Name,
                ["cus_email"] = donor.Email,
                ["cus_add1"] = donor.Address ?? "Dhaka",
                ["cus_add2"] = "",
                ["cus_city"] = "Dhaka",
                ["cus_state"] = "Dhaka",
                ["cus_postcode"] = "1000",
                ["cus_country"] = "Bangladesh",
                ["cus_phone"] = donor.Phone,
                ["cus_fax"] = "",
                ["ship_name"] = donor.Name,
                ["ship_add1"] = donor.Address ?? "Dhaka",
                ["ship_add2"] = "",
                ["ship_city"] = "Dhaka",
                ["ship_state"] = "Dhaka",
                ["ship_postcode"] = "1000",
                ["ship_country"] = "Bangladesh",
                ["value_a"] = donor.Id.ToString(),
                ["value_b"] = donor.DonationType ?? "General",
                ["value_c"] = donor.Message ?? "",
                ["value_d"] = donor.ReceiptNumber,
                ["success_url"] = $"{Request.Scheme}://{Request.Host}/Donation/PaymentSuccess",
                ["fail_url"] = $"{Request.Scheme}://{Request.Host}/Donation/PaymentFail",
                ["cancel_url"] = $"{Request.Scheme}://{Request.Host}/Donation/PaymentCancel",
                ["ipn_url"] = $"{Request.Scheme}://{Request.Host}/Donation/IPN"
            };

            try
            {
                // Call SSL Commerz API to create session
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
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
                        else
                        {
                            // Log the response for debugging
                            TempData["Error"] = "Payment gateway returned an invalid response. Please try again.";
                            return RedirectToAction("Index", "Home", new { area = "" });
                        }
                    }
                    else
                    {
                        TempData["Error"] = $"Payment gateway error: HTTP {response.StatusCode}";
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                TempData["Error"] = "Unable to connect to payment gateway. Please try again.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Fallback - show the payment form with manual submission
            ViewBag.PaymentData = paymentData;
            ViewBag.SessionApiUrl = sessionApiUrl;
            ViewBag.IsSandbox = isSandbox;
            ViewBag.StoreName = storeName;
            ViewBag.RegisteredUrl = registeredUrl;
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
                    if (donor.PaymentStatus == "Completed")
                    {
                        await NotifyDonorAsync(donor);
                    }
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
                // Handle both GET and POST requests from SSL Commerz
                var tranId = Request.Form["tran_id"].ToString() ?? Request.Query["tran_id"].ToString() ?? "";
                var status = Request.Form["status"].ToString() ?? Request.Query["status"].ToString() ?? "";
                var bankTranId = Request.Form["bank_tran_id"].ToString() ?? Request.Query["bank_tran_id"].ToString() ?? "";
                var donorIdStr = Request.Form["value_a"].ToString() ?? Request.Query["value_a"].ToString() ?? "";
                
                if (string.IsNullOrEmpty(tranId))
                {
                    TempData["Error"] = "Invalid transaction ID.";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                // Try to find donor by transaction ID first
                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.TransactionId == tranId);
                
                // If not found by transaction ID, try to find by the value_a field (donor ID)
                if (donor == null && !string.IsNullOrEmpty(donorIdStr) && int.TryParse(donorIdStr, out int donorId))
                {
                    donor = await _context.Donors.FindAsync(donorId);
                }
                
                if (donor != null)
                {
                    // For sandbox testing, we'll skip validation and mark as successful
                    var isSandbox = _configuration["SSLCommerz:IsSandbox"] == "true";
                    
                    if (isSandbox)
                    {
                        // In sandbox mode, accept the payment without validation
                        donor.PaymentStatus = "Completed";
                        
                        donor.TransactionId = !string.IsNullOrEmpty(bankTranId) ? bankTranId : tranId;
                        await _context.SaveChangesAsync();
                        await NotifyDonorAsync(donor);
                        
                        TempData["Success"] = "Payment successful! Thank you for your donation.";
                        return RedirectToAction("DonationSuccess", new { id = donor.Id });
                    }
                    else
                    {
                        // In production, validate the transaction with SSL Commerz
                        var isValid = await ValidateSSLCommerzTransaction(tranId);
                        
                        if (isValid || status == "VALID")
                        {
                            donor.PaymentStatus = "Completed";
                            donor.TransactionId = !string.IsNullOrEmpty(bankTranId) ? bankTranId : tranId;
                            await _context.SaveChangesAsync();
                            await NotifyDonorAsync(donor);
                            
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

        private async Task NotifyDonorAsync(Donor donor)
        {
            try
            {
                // SMS
                if (!string.IsNullOrWhiteSpace(donor.Phone))
                {
                    var sms = $"Thank you, {donor.Name}, for your generous donation of BDT {donor.Amount:F2}. Receipt: {donor.ReceiptNumber}. - Sector 13 Welfare Society";
                    await _smsSender.SendAsync(donor.Phone, sms);
                }

                // Email
                if (!string.IsNullOrWhiteSpace(donor.Email))
                {
                    var subject = $"Receipt for your donation - {donor.ReceiptNumber}";
                    var bodyIntro = $@"<p>Dear {donor.Name},</p>
<p>Thank you for your generous donation of <strong>BDT {donor.Amount:F2}</strong>.</p>
<p>Your official receipt is attached.</p>
<p>With gratitude,<br/>Sector 13 Welfare Society</p>";

                    var receiptHtml = GenerateReceiptHtml(donor);
                    var attachmentBytes = Encoding.UTF8.GetBytes(receiptHtml);
                    await _emailService.SendEmailWithAttachmentAsync(
                        donor.Email,
                        subject,
                        bodyIntro,
                        attachmentBytes,
                        $"Receipt_{donor.ReceiptNumber}.html",
                        "text/html");
                }
            }
            catch
            {
                // Best-effort; ignore notification errors
            }
        }

        private string GenerateReceiptHtml(Donor donor)
        {
            var amount = donor.Amount.ToString("N2");
            var date = donor.DonationDate.ToString("dd MMM yyyy");
            var time = donor.DonationDate.ToString("hh:mm tt");
            var txn = string.IsNullOrEmpty(donor.TransactionId) ? "N/A" : donor.TransactionId;
            var donorName = donor.IsAnonymous ? "Anonymous Donor" : donor.Name;
            var donationType = donor.DonationType ?? "General";

            return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1'>
  <title>Donation Receipt - {donor.ReceiptNumber}</title>
  <style>
    body {{ margin:0; padding:0; background:#f5f7fb; font-family: Arial, Helvetica, sans-serif; color:#1f2937; }}
    .wrapper {{ width:100%; padding:24px 12px; }}
    .receipt {{ max-width:720px; margin:0 auto; background:#ffffff; border:1px solid #e5e7eb; border-radius:12px; overflow:hidden; box-shadow:0 8px 24px rgba(16,24,40,.08); }}
    .header {{ padding:24px; background: linear-gradient(135deg,#0ea5e9 0%, #22c55e 100%); color:#fff; position:relative; }}
    .brand {{ font-size:22px; font-weight:800; letter-spacing:.2px; }}
    .tagline {{ opacity:.95; font-size:12px; margin-top:4px; }}
    .rmeta {{ margin-top:10px; font-size:12px; opacity:.95; }}
    .badge-paid {{ position:absolute; right:24px; top:24px; background:rgba(255,255,255,.15); border:1px solid rgba(255,255,255,.35); padding:6px 10px; font-weight:700; border-radius:999px; font-size:12px; text-transform:uppercase; }}
    .amount-bar {{ text-align:center; padding:18px; background:#f0f9ff; border-bottom:1px solid #e5e7eb; }}
    .amount-label {{ color:#0ea5e9; font-weight:700; font-size:12px; letter-spacing:.6px; text-transform:uppercase; }}
    .amount-value {{ display:block; margin-top:6px; font-size:32px; font-weight:800; color:#16a34a; }}
    .content {{ padding:24px; }}
    .grid {{ display:flex; gap:16px; flex-wrap:wrap; }}
    .col {{ flex:1 1 300px; border:1px solid #e5e7eb; border-radius:10px; padding:16px; background:#fafcff; }}
    .section-title {{ font-size:13px; font-weight:800; color:#334155; margin-bottom:10px; letter-spacing:.4px; text-transform:uppercase; }}
    .row {{ display:flex; justify-content:space-between; margin:6px 0; font-size:14px; }}
    .label {{ color:#6b7280; font-weight:600; }}
    .value {{ color:#111827; font-weight:600; text-align:right; }}
    .note {{ margin-top:14px; font-size:13px; color:#374151; background:#f9fafb; border:1px dashed #e5e7eb; padding:10px; border-radius:8px; }}
    .footer {{ padding:18px 24px; background:#f9fafb; border-top:1px solid #e5e7eb; text-align:center; color:#64748b; font-size:12px; }}
    .divider {{ height:1px; background:#e5e7eb; margin:16px 0; }}
    .impact {{ margin-top:8px; font-size:12px; color:#059669; font-weight:700; }}
  </style>
  <!-- Email clients prefer inline CSS; kept minimal and self-contained -->
  </head>
<body>
  <div class='wrapper'>
    <div class='receipt'>
      <div class='header'>
        <div class='brand'>উত্তরা সেক্টর ১৩ – ওয়েলফেয়ার সোসাইটি</div>
        <div class='tagline'>Together we make a difference • আপনার সহায়তায় আমরা এগিয়ে যাই</div>
        <div class='rmeta'>Receipt No: <strong>{donor.ReceiptNumber}</strong> • Date: {date} • Time: {time}</div>
        <div class='badge-paid'>Paid</div>
      </div>

      <div class='amount-bar'>
        <span class='amount-label'>Donation Amount</span>
        <span class='amount-value'>৳ {amount}</span>
        <div class='impact'>Your contribution supports community welfare programs.</div>
      </div>

      <div class='content'>
        <div class='grid'>
          <div class='col'>
            <div class='section-title'>Donor Details</div>
            <div class='row'><span class='label'>Name</span><span class='value'>{donorName}</span></div>
            <div class='row'><span class='label'>Email</span><span class='value'>{donor.Email}</span></div>
            <div class='row'><span class='label'>Phone</span><span class='value'>{donor.Phone}</span></div>
          </div>
          <div class='col'>
            <div class='section-title'>Transaction</div>
            <div class='row'><span class='label'>Transaction ID</span><span class='value'>{txn}</span></div>
            <div class='row'><span class='label'>Purpose</span><span class='value'>{donationType}</span></div>
            <div class='row'><span class='label'>Receipt</span><span class='value'>{donor.ReceiptNumber}</span></div>
          </div>
        </div>

        {(string.IsNullOrWhiteSpace(donor.Message) ? "" : $"<div class='note'><strong>Donor Message:</strong> {System.Net.WebUtility.HtmlEncode(donor.Message)}</div>")}

        <div class='divider'></div>
        <div style='font-size:12px; color:#6b7280;'>
          This receipt acknowledges your donation to Sector 13 Welfare Society. No goods or services were provided in exchange for this donation.
        </div>
      </div>

      <div class='footer'>
        Sector 13 Welfare Society • Uttara, Dhaka • www.wfs-13.org • Thank you for your kindness 💚
      </div>
    </div>
  </div>
</body>
</html>";
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
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var validationData = new Dictionary<string, string>
                    {
                        ["store_id"] = storeId,
                        ["store_passwd"] = storePassword,
                        ["tran_id"] = tranId,
                        ["val_id"] = tranId
                    };

                    var content = new FormUrlEncodedContent(validationData);
                    var response = await client.PostAsync(validationApiUrl, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Parse the response to check if transaction is valid
                    // SSL Commerz validation API returns different formats
                    if (response.IsSuccessStatusCode)
                    {
                        // Check for various success indicators in the response
                        var successIndicators = new[] { "VALID", "success", "SUCCESS", "approved", "APPROVED" };
                        var failureIndicators = new[] { "FAILED", "failed", "INVALID", "invalid", "cancelled", "CANCELLED" };
                        
                        var responseUpper = responseContent.ToUpper();
                        
                        // Check for success indicators
                        if (successIndicators.Any(indicator => responseUpper.Contains(indicator.ToUpper())))
                        {
                            return true;
                        }
                        
                        // Check for failure indicators
                        if (failureIndicators.Any(indicator => responseUpper.Contains(indicator.ToUpper())))
                        {
                            return false;
                        }
                        
                        // If response contains transaction details, consider it valid
                        if (responseContent.Contains("tran_id") || responseContent.Contains("amount") || responseContent.Contains("status"))
                        {
                            return true;
                        }
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                return false;
            }
        }

        // Debug method to test SSL Commerz configuration
        [HttpGet]
        public async Task<IActionResult> DebugSSLCommerz()
        {
            try
            {
                var storeId = _configuration["SSLCommerz:StoreId"] ?? "";
                var storePassword = _configuration["SSLCommerz:StorePassword"] ?? "";
                var sessionApiUrl = _configuration["SSLCommerz:SessionApiUrl"] ?? "";
                var isSandbox = _configuration["SSLCommerz:IsSandbox"] == "true";
                var storeName = _configuration["SSLCommerz:StoreName"] ?? "";
                var registeredUrl = _configuration["SSLCommerz:RegisteredUrl"] ?? "";

                var debugInfo = new
                {
                    StoreId = !string.IsNullOrEmpty(storeId) ? "Configured" : "Missing",
                    StorePassword = !string.IsNullOrEmpty(storePassword) ? "Configured" : "Missing",
                    SessionApiUrl = !string.IsNullOrEmpty(sessionApiUrl) ? sessionApiUrl : "Missing",
                    IsSandbox = isSandbox,
                    StoreName = storeName,
                    RegisteredUrl = registeredUrl,
                    CurrentUrl = $"{Request.Scheme}://{Request.Host}",
                    Timestamp = DateTime.Now
                };

                return Json(debugInfo);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message, Timestamp = DateTime.Now });
            }
        }
    }
} 