using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface ILotTransferService
    {
        public Task<BaseResponse> CreateLotTransfer(Guid accountId, LotTransferRequest request);
        public Task<byte[]> ExportLotTransfer(Guid accountId, int lotTransferId);
        public Task<BaseResponse> ApproveLotTransfer(Guid accountId, int lotTransferId);
    }
}
