using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateProviderRequest
    {
        [Required]
        public string ProviderName { get; set; } = null!;
        [Required]
        public string Address { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string PhoneNumber { get; set; } = null!;
        public string? TaxCode { get; set; }
        public string? Nationality { get; set; }
       
        public string DocumentNumber { get; set; } = null!; // Số chứng từ của nhà cung cấp
        public DateOnly DocumentIssueDate { get; set; } // Ngày cấp chứng từ

    }
}
