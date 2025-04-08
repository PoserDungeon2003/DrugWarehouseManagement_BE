using DrugWarehouseManagement.Common;
using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateOutboundRequest
    {
        public string? CustomerName { get; set; }
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits and contain only numbers.")]
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? OutboundOrderCode { get; set; }
        public string? Note { get; set; }
        public OutboundStatus? Status { get; set; }

    }
}
