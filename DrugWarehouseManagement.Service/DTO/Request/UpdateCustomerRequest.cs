using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateCustomerRequest
    {
        public string? CustomerName { get; set; }
        public string? Address { get; set; }
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits and contain only numbers.")]
        public string? PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }
        public string? DocumentNumber { get; set; } // Số chứng từ của khách hàng   
        public string? Status { get; set; } // Trạng thái của khách hàng
    }
}
