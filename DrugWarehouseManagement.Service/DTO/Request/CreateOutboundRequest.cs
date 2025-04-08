using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateOutboundRequest
    {

        [Required]    
        public int CustomerId { get; set; }     
        public string? ReceiverName { get; set; }
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Không đúng format số điện thoại.")]
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
    }
}
