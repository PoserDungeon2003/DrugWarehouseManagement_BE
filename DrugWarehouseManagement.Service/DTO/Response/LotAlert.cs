using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class LotAlert
    {
        public int LotId { get; set; }
        public string LotNumber { get; set; } = null!;
        public string AlertType { get; set; } = null!; //"LOW_STOCK" / "NEAR_EXPIRY_12M" / "NEAR_EXPIRY_60P"
        public string Message { get; set; } = null!;
    }
}
