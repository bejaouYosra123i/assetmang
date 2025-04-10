using System.ComponentModel.DataAnnotations;
namespace ITAssetManagement1.Models
{
    public class InvestmentItem
    {
        public int Id { get; set; }

        [Required]
        public string Item { get; set; } // Ensure this property exists

        [Required]
        public string Description { get; set; }

        [Required]
        public string Supplier { get; set; }

        [Required]
        public decimal UnitCost { get; set; }

        [Required]
        public decimal Shipping { get; set; }

        public decimal Subtotal => UnitCost + Shipping;

        [Required]
        public int Quantity { get; set; }

        public decimal Total => Subtotal * Quantity;

        public int InvestmentRequestId { get; set; }
        public InvestmentRequest InvestmentRequest { get; set; }
    }
}
