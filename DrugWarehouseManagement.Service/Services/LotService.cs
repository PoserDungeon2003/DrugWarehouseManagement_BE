using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;

namespace DrugWarehouseManagement.Service.Services
{
    public class LotService : ILotService
    {
        private readonly IUnitOfWork _unitOfWork;
        public LotService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<BaseResponse> CreateLot(CreateLotRequest request)
        {
            var lot = new Lot
            {
                LotNumber = request.LotNumber,
                TemporaryWarehouse = request.TemporaryWarehouse,
                WarehouseId = request.WarehouseId,
                ProductId = request.ProductId,
                ExpiryDate = request.ExpiryDate
            };

            await _unitOfWork.LotRepository.CreateAsync(lot);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Lot created successfully" };
        }

        public async Task<BaseResponse> UpdateLot(UpdateLotRequest request)
        {
            var lot = await _unitOfWork.LotRepository.GetByIdAsync(request.LotId);
            if (lot == null)
            {
                return new BaseResponse { Code = 404, Message = "Lot not found" };
            }

            lot.LotNumber = string.IsNullOrWhiteSpace(request.LotNumber) ? lot.LotNumber : request.LotNumber;
            lot.TemporaryWarehouse = request.TemporaryWarehouse != 0 ? request.TemporaryWarehouse : lot.TemporaryWarehouse;
            lot.WarehouseId = request.WarehouseId != 0 ? request.WarehouseId : lot.WarehouseId;
            lot.ProductId = request.ProductId != 0 ? request.ProductId : lot.ProductId;
            lot.ExpiryDate = request.ExpiryDate != default ? request.ExpiryDate : lot.ExpiryDate;


            await _unitOfWork.LotRepository.UpdateAsync(lot);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Lot updated successfully" };
        }

        public async Task<BaseResponse> DeleteLot(int lotId)
        {
            var lot = await _unitOfWork.LotRepository.GetByIdAsync(lotId);
            if (lot == null)
            {
                return new BaseResponse { Code = 404, Message = "Lot not found" };
            }

            await _unitOfWork.LotRepository.DeleteAsync(lot);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Lot deleted successfully" };
        }

        public async Task<ViewLot> GetLotById(int lotId)
        {
            var lot = await _unitOfWork.LotRepository.GetByIdAsync(lotId);
            if (lot == null)
            {
                throw new Exception("Lot not found");
            }
            return new ViewLot
            {
                LotId = lot.LotId,
                LotNumber = lot.LotNumber,
                TemporaryWarehouse = string.Empty,
                WarehouseName = lot.Warehouse.WarehouseName,
                ProductName = lot.Products.ProductName,
                ExpiryDate = lot.ExpiryDate
            };
        }

        public async Task<PaginatedResult<ViewLot>> GetLotsPaginatedAsync(QueryPaging request)
        {
            request.Search = request.Search?.ToLower().Trim() ?? "";
            var query = await _unitOfWork.LotRepository.GetAll()
                        .Where(x => x.LotNumber.Contains(request.Search) || x.TemporaryWarehouse.Equals(request.Search) || x.WarehouseId.Equals(request.Search))
                        .ToPaginatedResultAsync(request.Page, request.PageSize);
            return query.Adapt<PaginatedResult<ViewLot>>();
        }


    }
}
