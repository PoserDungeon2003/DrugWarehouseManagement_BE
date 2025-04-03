using System.ComponentModel.DataAnnotations;
using System;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateProviderRequest
    {
        public string? ProviderName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? TaxCode { get; set; }
        public string? Nationality { get; set; }
        public string? Email { get; set; }
        public string? DocumentNumber { get; set; }
        public DateOnly? DocumentIssueDate { get; set; }
        public string? Status { get; set; } // Trạng thái của nhà cung cấp
    }
}
