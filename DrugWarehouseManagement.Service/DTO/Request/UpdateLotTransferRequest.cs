using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateLotTransferRequest
    {
        [Required]
        public int LotTransferId { get; set; }
        public string? LotTransferCode { get; set; }
        public LotTransferStatus? LotTransferStatus { get; set; }
        public int? ToWareHouseId { get; set; }
        public List<UpdateLotTransferDetailRequest>? LotTransferDetails { get; set; }
    }

    public class UpdateLotTransferDetailRequest
    {

    }
}
