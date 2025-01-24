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
    public class AuditLogs
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditId { get; set; }
        public Instant Date { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public string Resource { get; set; } = null!;
        public string Action { get; set; } = null!;
        public Dictionary<string, object> Payload { get; set; } = null!;
        public Guid AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;
    }
}
