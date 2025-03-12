using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Service.DTO.Request;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewInbound
    {
        public int InboundId { get; set; }
        public string? InboundCode { get; set; }
        public string? ProviderOrderCode { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Note { get; set; }
        public string? ProviderName { get; set; }
        public string? ProductName { get; set; }
        public string? WareHouseName { get; set; }
        public string? InboundDate { get; set; } // Ngày nhập kho
        public InboundStatus Status { get; set; }
    }

    public class InboundDetailResponse
    {
        public string? ProductName { get; set; }
        public string? LotNumber { get; set; }
        public Instant ExpiryDate { get; set; }
        public int? Quantity { get; set; }
        public int? ActualQuantity { get; set; }
        public string? UnitType { get; set; } = null!;
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
