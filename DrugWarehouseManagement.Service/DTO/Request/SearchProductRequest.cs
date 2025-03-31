using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class SearchProductRequest : QueryPaging
    {
        public int? CategoryId { get; set; }
        public string? Status { get; set; }
    }
}
