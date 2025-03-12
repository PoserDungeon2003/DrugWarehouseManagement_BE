using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrugWarehouseManagement.Common;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Asset
    {
        [Key]
        public int AssetId { get; set; }
        public string FileUrl { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FileExtension { get; set; } = null!;
        public long FileSize { get; set; }
        public Instant UploadedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public AssetStatus Status { get; set; } = AssetStatus.Active;
        public Guid AccountId { get; set; }
        public int CategoryId { get; set; }

        public List<Inbound> Inbounds { get; set; } = null!;
        public List<InboundReport> InboundReports { get; set; } = null!;
        public virtual List<Provider> Providers { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual Categories Category { get; set; } = null!;
    }
}
