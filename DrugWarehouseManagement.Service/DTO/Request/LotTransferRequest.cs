using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class LotTransferRequest
    {
        public int FromWareHouseId { get; set; }
        public int ToWareHouseId { get; set; }  
        [Required]
        public List<LotTransferDetailRequest> LotTransferDetails { get; set; } = null!;
    }

    public class LotTransferDetailRequest
    {
        public DateOnly ExpiryDate { get; set; }
        public string UnitType { get; set; } = null!;
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int LotId { get; set; }
        public string NewLotNumber { get; set; } = null!;
    }
}
