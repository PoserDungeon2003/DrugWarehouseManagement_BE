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
        [Range(1, int.MaxValue, ErrorMessage = "FromWareHouseId must be a positive number.")]
        public int FromWareHouseId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ToWareHouseId must be a positive number.")]
        public int ToWareHouseId { get; set; }
        [Required]
        public List<LotTransferDetailRequest> LotTransferDetails { get; set; } = null!;
    }

    public class LotTransferDetailRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int Quantity { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "LotId must be a positive number.")]
        public int LotId { get; set; }
    }
}
