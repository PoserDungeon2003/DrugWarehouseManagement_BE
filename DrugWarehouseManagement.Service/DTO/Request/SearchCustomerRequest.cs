using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class SearchCustomerRequest : QueryPaging
    {
        public int? CustomerId { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
