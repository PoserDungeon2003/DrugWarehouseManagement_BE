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
        [MaxLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
        public string? Note { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProviderId must be greater than 0.")]
        public int ProviderId { get; set; }
        public int? WarehouseId { get; set; } // Warehouse
        public List<InboundDetailRequest>? InboundDetails { get; set; }

    }

    public class UpdateInboundStatusRequest 
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "InboundId must be greater than 0.")]
        public int InboundId { get; set; }
        public InboundStatus InboundStatus { get; set; }
    }

}
