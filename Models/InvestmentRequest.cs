namespace ITAssetManagement1.Models
{
    public class InvestmentRequest
    {
        public int Id { get; set; }
        public string Region { get; set; } // e.g., "YEL"
        public string Currency { get; set; } // e.g., "EUR"
        public string Location { get; set; }
        public string TypeOfInvestment { get; set; } // e.g., "New", "Refresh"
        public string Justification { get; set; } // e.g., "replacement for old ones"
        public DateTime RequestedDate { get; set; } // e.g., "9/16/2024"
        public DateTime DueDate { get; set; }
        public decimal Total { get; set; } // Total cost of all items
        public int CreatedById { get; set; } // Foreign key to Profile
        public Profile CreatedBy { get; set; } // Navigation property
        public DateTime CreationDate { get; set; }
        public string Status { get; set; } // "Pending", "Approved", "Rejected"
        public string Type => Total < 1000 ? "Invest < 1000€" : "Invest > 1000€"; // Computed property based on Total
        public List<InvestmentItem> Items { get; set; } = new List<InvestmentItem>();
    }
}
