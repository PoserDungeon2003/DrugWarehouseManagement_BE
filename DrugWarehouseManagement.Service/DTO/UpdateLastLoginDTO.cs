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
        public DateTime LastLogin { get; set; }
    }
}
