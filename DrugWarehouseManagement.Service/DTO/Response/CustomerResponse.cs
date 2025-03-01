using DrugWarehouseManagement.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class CustomerResponse
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsLoyal { get; set; }
        public CustomerStatus Status { get; set; }
    }
}
