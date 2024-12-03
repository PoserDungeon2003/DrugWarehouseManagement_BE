using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO
{
    public class UpdateLastLoginDTO
    {
        public Guid AccountId { get; set; }
        public Instant LastLogin { get; set; }
    }
}
