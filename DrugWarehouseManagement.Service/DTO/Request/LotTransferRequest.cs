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
        [Required]
        public string LotTransferCode { get; set; } = null!;
        [Required]
        public int FromWareHouseId { get; set; }
        [Required]
        public int ToWareHouseId { get; set; }  
        [Required]
        public List<LotTransferDetailRequest> LotTransferDetails { get; set; } = null!;
    }

    public class LotTransferDetailRequest
    {
        [Required]
        public int Quantity { get; set; }
        [Required]
        public int LotId { get; set; }
    }
}
