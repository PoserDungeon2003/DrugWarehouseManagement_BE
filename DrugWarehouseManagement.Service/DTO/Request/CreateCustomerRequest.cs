using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateCustomerRequest
    {
        [Required]
        public string CustomerName { get; set; } = null!;

        public string? Address { get; set; }
        [MaxLength(15, ErrorMessage = "Max length is 15")]
        [MinLength(10, ErrorMessage = "Min length is 10")]
        [RegularExpression(@"^[\d+]*$", ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string DocumentNumber { get; set; } = null!;
    }
}
