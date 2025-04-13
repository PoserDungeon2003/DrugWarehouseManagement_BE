using DrugWarehouseManagement.Common;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateInboundRequest
    {
        public string? ProviderOrderCode { get; set; }
        [MaxLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
        public string? Note { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProviderId must be greater than 0.")]
        public int ProviderId { get; set; }
        public int? WarehouseId { get; set; }
        public int? InboundRequestId { get; set; }
        public List<InboundDetailRequest>? InboundDetails { get; set; }
    }

    public class InboundDetailRequest
    {
        [Required(ErrorMessage = "LotNumber is required.")]
        [StringLength(50, ErrorMessage = "LotNumber must be at most 50 characters.")]
        public string LotNumber { get; set; } = null!;
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int ProductId { get; set; }
        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }


        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "UnitPrice is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "TotalPrice is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "TotalPrice must be greater than 0.")]
        public decimal TotalPrice { get; set; }
    }

}
