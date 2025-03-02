using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface ITransferOrderService
    {
        public Task<BaseResponse> CreateTransferOrder(Guid accountId, TransferOrderRequest request);
        public Task<byte[]> ExportTransferOrder(Guid accountId, int transferOrderId);
        public Task<BaseResponse> ApproveTransferOrder(Guid accountId, int transferOrderId);
    }
}
