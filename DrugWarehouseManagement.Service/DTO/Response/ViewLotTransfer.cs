using DrugWarehouseManagement.Common;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewLotTransfer
    {
        public int LotTransferId { get; set; }
        public string LotTransferCode { get; set; } = null!;
        public string LotTransferStatus { get; set; } = null!;
        public string FromWareHouse { get; set; } = null!;
        public string ToWareHouse { get; set; } = null!;
        public string CreatedBy { get; set; } = null!;
        public Instant CreatedAt { get; set; }
        public Instant? UpdatedAt { get; set; }
    }
}
