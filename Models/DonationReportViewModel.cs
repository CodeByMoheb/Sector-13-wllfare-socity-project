using System;
using System.Collections.Generic;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class DonationReportFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
        public string? DonationType { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
    }

    public class TopDonorItem
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Donations { get; set; }
    }

    public class DonationReportViewModel
    {
        public DonationReportFilter Filter { get; set; } = new DonationReportFilter();

        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public decimal TodayAmount { get; set; }
        public decimal ThisWeekAmount { get; set; }
        public decimal ThisMonthAmount { get; set; }
        public decimal AveragePerDonation { get; set; }

        public List<TopDonorItem> TopDonors { get; set; } = new List<TopDonorItem>();
        public PaginatedList<Donor> Donations { get; set; } = null!;

        public List<string> AvailablePaymentMethods { get; set; } = new List<string>();
        public List<string> AvailableStatuses { get; set; } = new List<string> { "Pending", "Completed", "Failed", "Cancelled" };
        public List<string> AvailableTypes { get; set; } = new List<string> { "General", "Healthcare", "Education", "Emergency" };
    }
}


