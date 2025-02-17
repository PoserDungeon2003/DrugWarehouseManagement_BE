using DrugWarehouseManagement.Common.Enums;
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
        public string? InboundCode { get; set; }
        public string? ProviderOrderCode { get; set; }
        public string? Note { get; set; }
        public InboundStatus? InboundStatus { get; set; }
    }
}
