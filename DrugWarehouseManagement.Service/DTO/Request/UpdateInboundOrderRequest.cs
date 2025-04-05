using DrugWarehouseManagement.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateInboundOrderRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "InboundOrderId must be greater than 0.")]
        public int InboundOrderId { get; set; }
        [MaxLength(1000, ErrorMessage = "Note must be at most 1000 characters.")]
        public string? Note { get; set; }
        public decimal? Price { get; set; }
        public List<InboundOrderDetailRequest>? InboundRequestDetails { get; set; }
        public List<IFormFile>? Images { get; set; }
    }

    public class UpdateInboundOrderStatusRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "InboundOrderId must be greater than 0.")]
        public int InboundId { get; set; }
        public InboundRequestStatus InboundOrderStatus { get; set; }
    }
}
