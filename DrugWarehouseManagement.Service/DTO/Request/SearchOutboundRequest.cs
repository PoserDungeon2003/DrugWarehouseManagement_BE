using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
     public class SearchOutboundRequest : QueryPaging
    {
        public int? CustomerId { get; set; }
        public string? Status { get; set; }
    }
}
