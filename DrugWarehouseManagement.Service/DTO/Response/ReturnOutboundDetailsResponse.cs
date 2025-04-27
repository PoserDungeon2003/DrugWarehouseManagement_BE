using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ReturnOutboundDetailsResponse
    {
        public int ReturnOutboundDetailsId { get; set; }
        public int OutboundDetailId { get; set; }
        public int ReturnedQuantity { get; set; }
        public Instant CreatedAt { get; set; }
        public string? Note { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? OutboundCode { get; set; }
      
    }
}
