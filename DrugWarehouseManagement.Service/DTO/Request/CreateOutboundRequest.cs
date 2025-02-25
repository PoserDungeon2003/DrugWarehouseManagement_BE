using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateOutboundRequest
    {

        [Required]
        public string CustomerName { get; set; } = null!;
        public int CustomerId { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OutboundOrderCode { get; set; }
        public string? TrackingNumber { get; set; }
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

        [Required]
        public string UnitType { get; set; } = null!;
    }
}
