using DrugWarehouseManagement.Common.Enums;
using NodaTime;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class OutboundDetailRespone
    {
        public int OutboundDetailsId { get; set; }
        public int LotId { get; set; }
        public string LotNumber { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string UnitType { get; set; } = null!;
        public Instant ExpiryDate { get; set; }
        public OutboundDetailStatus Status { get; set; }
        public int ProductId { get; set; }
    }
    public class OutboundResponse
    {
        public int OutboundId { get; set; }
        public string OutboundCode { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OutboundOrderCode { get; set; }
        public string? TrackingNumber { get; set; }
        public Instant? OutboundDate { get; set; }
        public OutboundStatus Status { get; set; }
        public List<OutboundDetailRespone> OutboundDetails { get; set; } = new List<OutboundDetailRespone>();
    }

}
