using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateInboundRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "InboundId must be greater than 0.")]
        public int InboundId { get; set; }
        public string? ProviderOrderCode { get; set; }

        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
        public string? Note { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ProviderId must be greater than 0.")]
        public int? ProviderId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int? Quantity { get; set; } // Số lượng

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal? Price { get; set; } // Giá

        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
        public int? ProductId { get; set; } // Product
        public int? WarehouseId { get; set; } // Warehouse
        public Guid AccountId { get; set; }
        public Instant? InboundDate { get; set; }

    }

    public class UpdateInboundStatusRequest 
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "InboundId must be greater than 0.")]
        public int InboundId { get; set; }
        public InboundStatus InboundStatus { get; set; }
        public Guid AccountId { get; set; }
    }

}
