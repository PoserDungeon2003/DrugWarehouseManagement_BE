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
    public class Asset
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssetId { get; set; }
        public string FileUrl { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileExtension { get; set; } = null!;
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public Instant UploadedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public AssetStatus Status { get; set; } = AssetStatus.Active;
        public Guid AccountId { get; set; }
        public int? CategoryId { get; set; }

        public List<InboundRequest> InboundRequests { get; set; } = null!;
        public List<InboundReport> InboundReports { get; set; } = null!;
        public virtual List<Provider> Providers { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual Categories? Category { get; set; }
        public virtual List<InboundReportAssets> InboundReportAssets { get; set; } = null!;
        public virtual List<InboundRequestAssets> InboundRequestAssets { get; set; } = null!;
        public virtual List<ProviderAssets> ProviderAssets { get; set; } = null!;
        public virtual LotTransfer? LotTransfer { get; set; }
    }
}
