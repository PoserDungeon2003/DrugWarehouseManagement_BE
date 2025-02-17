using DrugWarehouseManagement.Common.Enums;
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
        public string? InboundCode { get; set; }
        public string? ProviderOrderCode { get; set; }
        public string? Note { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProviderId must be greater than 0.")]
        public int ProviderId { get; set; }
        public Guid AccountId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one inbound detail is required.")]
        public List<InboundDetailList> InboundDetailList { get; set; } = new List<InboundDetailList>();
    }

    public class InboundDetailList
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int ProductId { get; set; }
        public string? LotNumber { get; set; }
        public Instant ExpiryDate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }
        public string UnitType { get; set; } = null!;
        public decimal UnitPrice { get; set; }
    }
}
