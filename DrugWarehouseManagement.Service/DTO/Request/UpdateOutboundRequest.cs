using DrugWarehouseManagement.Common;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateOutboundRequest
    {
        public string? CustomerName { get; set; } = null!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OutboundOrderCode { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Note { get; set; }
        public OutboundStatus? Status { get; set; }

    }
}
