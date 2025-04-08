using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewInboundRequest
    {
        public int InboundRequestId { get; set; }
        public string InboundRequestCode { get; set; }
        public string? Note { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public string CreateDate { get; set; }
        public List<InboundRequestDetailResponse> InboundRequestDetails { get; set; }
        public List<AssetResponse> Assets { get; set; }
    }

    public class InboundRequestDetailResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class AssetResponse
    {
        public int AssetId { get; set; }
        public string FileUrl { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileExtension { get; set; } = null!;
        public long FileSize { get; set; }
        public string UploadedAt { get; set; } = null!;
        public string Status { get; set; } = null!;
        public Guid AccountId { get; set; }
        public int CategoryId { get; set; }
    }
}
