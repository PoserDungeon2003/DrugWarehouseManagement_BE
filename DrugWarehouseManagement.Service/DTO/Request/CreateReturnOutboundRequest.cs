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
        public int? ToWarehouseId { get; set; }

        // Danh sách dòng chi tiết trả về
        public List<ReturnOutboundDetailItem> Details { get; set; } = new();
    }
    public class ReturnOutboundDetailItem
    {
        public int OutboundDetailsId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }
}
