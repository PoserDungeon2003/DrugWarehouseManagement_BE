using DrugWarehouseManagement.Common;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Inbound : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InboundId { get; set; }
        public string? InboundCode { get; set; } // Mã phiếu
        public string? ProviderOrderCode { get; set; } // Mã đơn hàng
        public string? Note { get; set; }
        public Instant? InboundDate { get; set; } // Ngày nhập kho
        public InboundStatus Status { get; set; } = InboundStatus.Pending;
        public int ProviderId { get; set; } // Provider
        public Guid AccountId { get; set; } // Created By
        public int WarehouseId { get; set; } // Warehouse
        public int? InboundRequestId { get; set; }


        public virtual List<InboundDetails> InboundDetails { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual Provider Provider { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual InboundRequest? InboundRequest { get; set; }
    }
}
