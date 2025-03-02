using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class CreateTransferOrderResponse
    {
        public int TransferOrderId { get; set; }
        public string TransferOrderCode { get; set; } = null!;
        public string TransferOrderStatus { get; set; } = null!;
        public string FromWareHouse { get; set; } = null!;
        public string ToWareHouse { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}
