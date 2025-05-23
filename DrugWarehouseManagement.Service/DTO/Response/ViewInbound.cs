﻿using DrugWarehouseManagement.Common;
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
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? CreateBy { get; set; }
        public string? Note { get; set; }
        public Instant? InboundDate { get; set; } // Ngày nhập kho
        public string Status { get; set; }
        public List<InboundDetailResponse> InboundDetails { get; set; }
        public ViewInboundReport? Report { get; set; }
        public ProviderResponse ProviderDetails { get; set; }
    }

    public class InboundDetailResponse
    {
        public string LotNumber { get; set; } = null!;
        public int? ProductId {  get; set; }
        public string? ProductName { get; set; }
        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
