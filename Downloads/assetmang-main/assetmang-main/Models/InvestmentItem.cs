namespace ITAssetManagement1.Models
{
    public class InvestmentItem
    {
        public int Id { get; set; }
        
        public string Item { get; set; } // e.g., "KNIPEX VDE Seitenschneider"
        public string Description { get; set; } // e.g., "180mm"
        public string Supplier { get; set; } // e.g., "IT-HAUS"
        public decimal subTotal { get; set; } // e.g., "IT-HAUS"
        public decimal Shipping { get; set; }
        public decimal UnitCost { get; set; } // e.g., 26.5
        public int Quantity { get; set; } // e.g., 1
        public decimal Total => UnitCost * Quantity; // No shipping per item
        public List<InvestmentItem> Items { get; set; } = new List<InvestmentItem>();
    }
}
