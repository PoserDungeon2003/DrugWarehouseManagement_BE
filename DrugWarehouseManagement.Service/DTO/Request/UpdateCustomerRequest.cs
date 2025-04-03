using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateCustomerRequest
    {
        public string? CustomerName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? DocumentNumber { get; set; } // Số chứng từ của khách hàng   
        public string? Status { get; set; } // Trạng thái của khách hàng
    }
}
