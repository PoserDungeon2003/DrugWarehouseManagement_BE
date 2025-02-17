using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateLotRequest
    {
        [Required]
        public string LotNumber { get; set; } = string.Empty;
        public int TemporaryWarehouse { get; set; }
        [Required]
        public int WarehouseId { get; set; } // Permanent Warehouse
        [Required]
        public Instant ExpiryDate { get; set; }
        [Required]
        public int ProductId { get; set; }
    }
}
