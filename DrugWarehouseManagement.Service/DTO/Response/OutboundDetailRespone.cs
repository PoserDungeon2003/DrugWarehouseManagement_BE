﻿using DrugWarehouseManagement.Common;
using NodaTime;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class OutboundDetailRespone
    {
        public int OutboundDetailsId { get; set; }
        public int LotId { get; set; }
        public string LotNumber { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string UnitType { get; set; } = null!;
        public string? ProductName { get; set; }
        public DateOnly ExpiryDate { get; set; }

        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = null!;
        public string WarehouseName { get; set; } = null!;
        public List<ReturnOutboundDetailsResponse> Returns { get; set; } = new();
    }
    public class OutboundResponse
    {
        public int OutboundId { get; set; }
        public string OutboundCode { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? ReceiverAddress { get; set; }
        public string? Note { get; set; }   
        public string? PhoneNumber { get; set; }
        public string? OutboundOrderCode { get; set; }
        public Instant? OutboundDate { get; set; }
        public OutboundStatus Status { get; set; }
        public List<OutboundDetailRespone> OutboundDetails { get; set; } = new List<OutboundDetailRespone>();
    }

}
