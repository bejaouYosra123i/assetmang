using System;
using System.Collections.Generic;
using System.Linq;

namespace ITAssetManagement1.Models
{
    public class InvestmentRequest : InvestmentItem
    {
        // Remove the Id property; it will be inherited from InvestmentItem
        public string Region { get; set; } // e.g., "YEL"
        public string Currency { get; set; } // e.g., "EUR"
        public string Location { get; set; }
        public string TypeOfInvestment { get; set; } // e.g., "New", "Refresh"
        public string Justification { get; set; } // e.g., "replacement for old ones"
        public DateTime RequestedDate { get; set; }
        public DateTime? DueDate { get; set; } // Nullable to match empty DueDate
        public string Status { get; set; } // "Pending", "Approved", "Rejected"
        public string Observation { get; set; }
        // Shipping is already inherited from InvestmentItem
        public decimal Total => (Items?.Sum(item => item.Total) ?? 0) + Shipping; // Include shipping
        public string Type => Total < 1000 ? "Invest < 1000€" : "Invest > 1000€";
    }
}