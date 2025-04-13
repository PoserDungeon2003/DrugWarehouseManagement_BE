using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateOutboundRequest
    {

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be a positive number.")]
        public int CustomerId { get; set; }
        public string? ReceiverName { get; set; }
        [MaxLength(15, ErrorMessage = "Max length is 15")]
        [MinLength(10, ErrorMessage = "Min length is 10")]
        [RegularExpression(@"^[\d+]*$", ErrorMessage = "Invalid phone number")]
        public string? ReceiverPhone { get; set; }
        public string? ReceiverAddress { get; set; }
        public string? OutboundOrderCode { get; set; }
        public string? Note { get; set; }

        [Required]
        public List<CreateOutboundDetailRequest> OutboundDetails { get; set; } = new();
    }

    public class CreateOutboundDetailRequest
    {
        [Required]
        public int LotId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be zero or a positive number.")]
        public decimal UnitPrice { get; set; }
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
        public float? Discount { get; set; }
        public bool UsePricingFormula { get; set; } = false; //áp d dụng công thức giá hay không 
        [Range(0, 1, ErrorMessage = "Profit margin must be between 0 and 1.")]
        public decimal? ProfitMargin { get; set; }  // ví dụ: 0.2 tương đương 20%
        [Range(0, 1, ErrorMessage = "Tax percentage must be between 0 and 1.")]
        public decimal? TaxPercentage { get; set; }   // ví dụ: 0.1 tương đương 10%

    }
}
