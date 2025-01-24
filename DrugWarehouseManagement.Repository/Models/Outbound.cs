using DrugWarehouseManagement.Common.Enums;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Outbound : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OutboundId { get; set; }
        public string OutboundCode { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? OutboundOrderCode { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Note { get; set; }
        public Instant? OutboundDate { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public OutboundStatus Status { get; set; } = OutboundStatus.Pending;
        public Guid AccountId { get; set; }

        public virtual List<OutboundDetails> OutboundDetails { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
    }
}
