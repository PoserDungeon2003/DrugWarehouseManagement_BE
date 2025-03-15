using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Column(TypeName = "jsonb")]
        public string Payload { get; set; } = null!;
        public Guid? AccountId { get; set; }

        public virtual Account? Account { get; set; } = null!;
    }
}
