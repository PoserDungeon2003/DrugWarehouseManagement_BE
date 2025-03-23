using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateReturnOutboundRequest
    {
        //tham chiếu OutboundId tổng thể (nếu 1 inbound trả lại nhiều detail)
        public int OutboundId { get; set; }

        // Danh sách dòng chi tiết trả về
        public List<ReturnOutboundDetailItem> Details { get; set; } = new();
    }
    public class ReturnOutboundDetailItem
    {
        public int OutboundDetailId { get; set; }
        public int InboundDetailId { get; set; }
        public int ReturnedQuantity { get; set; }
        public string? Note { get; set; }
    }
}
