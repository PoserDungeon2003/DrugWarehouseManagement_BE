using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewAuditLogs
    {
        public int AuditId { get; set; }
        public Instant Date { get; set; }
        public string Resource { get; set; } = null!;
        public string Action { get; set; } = null!;
        public Dictionary<string, object> Payload { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public Guid AccountId { get; set; }
    }
}
