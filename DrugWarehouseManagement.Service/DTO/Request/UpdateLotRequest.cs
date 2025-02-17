using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateLotRequest
    {
        public int LotId { get; set; }
        public string LotNumber { get; set; } = null!;
        public int TemporaryWarehouse { get; set; }
        public int WarehouseId { get; set; } // Permanent Warehouse
        public Instant ExpiryDate { get; set; }
        public int ProductId { get; set; }
    }
}
