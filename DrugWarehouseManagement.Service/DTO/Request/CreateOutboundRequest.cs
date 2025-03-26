using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateOutboundRequest
    {

        [Required]    
        public int CustomerId { get; set; }     
        public string? ReceiverName { get; set; }
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
        public decimal UnitPrice { get; set; }

        public float? Discount { get; set; } 
    }
}
