using DrugWarehouseManagement.Common;
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
    }

    public class InboundRequestDetailResponse
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
