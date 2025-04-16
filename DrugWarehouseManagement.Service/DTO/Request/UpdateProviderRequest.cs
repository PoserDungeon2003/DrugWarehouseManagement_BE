using System.ComponentModel.DataAnnotations;
using System;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateProviderRequest
    {
        public string? ProviderName { get; set; }
        public string? Address { get; set; }
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits and contain only numbers.")]
        public string? PhoneNumber { get; set; }
        public string? TaxCode { get; set; }
        public string? Nationality { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Status { get; set; } // Trạng thái của nhà cung cấp
    }
}
