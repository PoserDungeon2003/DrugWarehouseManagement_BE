using DrugWarehouseManagement.Common.Enums;
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
        public string InboundCode { get; set; } = null!; // Mã phiếu
        public string ProviderOrderCode { get; set; } = null!; // Mã đơn hàng
        public string? Note { get; set; }
        public Instant? InboundDate { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public InboundStatus Status { get; set; } = InboundStatus.Pending;
        public int ProviderId { get; set; } // Provider
        public Guid AccountId { get; set; } // Created By

        public virtual List<InboundDetails> InboundDetails { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual Provider Provider { get; set; } = null!;
    }
}
