using DrugWarehouseManagement.Common;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Outbound : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OutboundId { get; set; }
        public string OutboundCode { get; set; } = null!;
        public int CustomerId { get; set; }
        public string? OutboundOrderCode { get; set; }
        public string? Note { get; set; }
        public Instant? OutboundDate { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public OutboundStatus Status { get; set; } = OutboundStatus.Pending;
        public Guid AccountId { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual List<OutboundDetails> OutboundDetails { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
    }
}
