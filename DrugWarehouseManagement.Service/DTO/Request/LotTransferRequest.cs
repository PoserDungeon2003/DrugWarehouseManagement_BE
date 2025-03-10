using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class LotTransferRequest
    {
        public string LotTransferCode { get; set; } = null!;
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
        public string LotNumber { get; set; } = null!;
        [JsonIgnore]
        public int LotId { get; set; }
    }
}
