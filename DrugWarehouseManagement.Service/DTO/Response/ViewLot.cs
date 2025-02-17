using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewLot
    {
        public int LotId { get; set; }
        public string LotNumber { get; set; } = null!;
        public string TemporaryWarehouse { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = null!; // Permanent Warehouse
        public string ProductName { get; set; } = null!;
        public Instant ExpiryDate { get; set; }
    }
}
