using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrugWarehouseManagement.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class InboundReport : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InboundReportId { get; set; }
        public Instant ReportDate { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public InboundReportStatus Status { get; set; } = InboundReportStatus.Pending;
        public string ProblemDescription { get; set; } = null!;
        public Guid AccountId { get; set; }
        public int InboundId { get; set; }

        public List<Asset> Assets { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual Inbound Inbound { get; set; } = null!;
        public virtual List<InboundReportAssets> InboundReportAssets { get; set; } = null!;
    }
}
