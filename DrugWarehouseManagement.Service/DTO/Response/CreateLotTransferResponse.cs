using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class CreateLotTransferResponse
    {
        public int LotTransferId { get; set; }
        public string LotTransferCode { get; set; } = null!;
        public string LotTransferStatus { get; set; } = null!;
        public string FromWareHouse { get; set; } = null!;
        public string ToWareHouse { get; set; } = null!;
        public string Username { get; set; } = null!;
        public List<CreateLotTransferDetailResponse> LotTransferDetails { get; set; } = null!;
    }

    public class CreateLotTransferDetailResponse
    {
        public int LotTransferDetailId { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public string UnitType { get; set; } = null!;
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int LotId { get; set; }
        public int LotTransferId { get; set; }
    }
}
