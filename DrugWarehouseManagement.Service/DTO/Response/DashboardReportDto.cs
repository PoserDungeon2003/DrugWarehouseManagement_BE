using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class DashboardReportDto
    {
        public int TotalOutboundOrders { get; set; }
        public int TotalInboundOrders { get; set; }
        public int TotalLotTransferOrders { get; set; }
        // **MỚI** Tổng giá tiền
        public decimal? TotalInboundValue { get; set; }        // tổng giá tiền nhập hàng
        public decimal? TotalOutboundValue { get; set; }       // tổng giá tiền xuất hàng
                                 
        public ProductStatisticDto ?BestStockedProduct { get; set; }

        public int OutboundCompletedCount { get; set; }
        public int OutboundCancelledCount { get; set; }
        public int OutboundSampleCount { get; set; }
        public int OutboundReturnedCount { get; set; }

        // Sản phẩm xuất - nhập nhiều nhất
        public ProductStatisticDto? BestExportedProduct { get; set; }
        public ProductStatisticDto? BestImportedProduct { get; set; }

        // Phân loại đơn nhập
        public InboundClassificationDto InboundClassification { get; set; }

      

        // Danh sách sản phẩm dưới mức quy định
        public List<ProductLowStockDto>? LowStockProducts { get; set; }
        // Danh sách đơn hàng theo trạng thái
        public OrderSummaryDto? OrderSummary { get; set; }

  

        public class ProductStatisticDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int TotalQuantity { get; set; }
        }

        public class InboundClassificationDto
        {
            public int InboundReturnCount { get; set; } // nhập trả (InboundRequestId == null)
            public int InboundNormalCount { get; set; } // nhập đơn hàng (InboundRequestId != null)
        }

        public class DocumentStatusDto
        {
            public int DocumentId { get; set; }
            public string DocumentType { get; set; } = string.Empty;// "Inbound", "Outbound", "LotTransfer", v.v.
            public string DocumentCode { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
        public List<DocumentStatusDto> NewDocuments { get; set; } = new();

        //inbound orders awaiting creation approval
        public List<OrderDto> NewInboundOrders { get; set; } = new();

        // Inbound‐request orders waiting for accountant
        public List<OrderDto> AccountantInboundOrders { get; set; } = new();

        //Inbound‐request orders waiting for director
        public List<OrderDto> DirectorInboundOrders { get; set; } = new();
        public class ProductLowStockDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int CurrentStock { get; set; }
            public int Threshold { get; set; }
        }

        public class OrderSummaryDto
        {
            public List<OrderDto> NewOrders { get; set; } = new();
            public List<OrderDto> ProcessingOrders { get; set; } = new();
        }

        public class OrderDto
        {
            public int OrderId { get; set; }
            public string OrderCode { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }

       
    }
}
