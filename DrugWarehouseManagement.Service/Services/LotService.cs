using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;

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
            // Ensure the foreign keys exist before creating the lot
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.ProductId);
            if (product == null) return new BaseResponse { Code = 404, Message = "Product not found" };

            var warehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(request.WarehouseId);
            if (warehouse == null) return new BaseResponse { Code = 404, Message = "Warehouse not found" };

            if (request.ProviderId > 0)
            {
                var provider = await _unitOfWork.ProviderRepository.GetByIdAsync(request.ProviderId);
                if (provider == null) return new BaseResponse { Code = 404, Message = "Provider not found" };
            }

            var lot = new Lot
            {
                LotNumber = request.LotNumber,
                Quantity = request.Quantity,
                WarehouseId = request.WarehouseId,
                ManufacturingDate = request.ManufacturingDate ?? default,
                ExpiryDate = request.ExpiryDate,
                ProductId = request.ProductId,
                ProviderId = request.ProviderId
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
            lot.Quantity = request.Quantity ?? lot.Quantity;
            lot.WarehouseId = request.WarehouseId ?? lot.WarehouseId;
            lot.ProductId = request.ProductId ?? lot.ProductId;
            lot.ProviderId = request.ProviderId ?? lot.ProviderId;
            lot.ManufacturingDate = request.ManufacturingDate ?? lot.ManufacturingDate;
            lot.ExpiryDate = request.ExpiryDate ?? lot.ExpiryDate;

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
                WarehouseName = lot.Warehouse.WarehouseName,
                ProductName = lot.Product.ProductName,
                ProviderName = lot.Provider.ProviderName,
                ExpiryDate = lot.ExpiryDate,
                ManufacturingDate = lot.ManufacturingDate
            };
        }

        public async Task<PaginatedResult<ViewLot>> GetLotsPaginatedAsync(QueryPaging request)
        {
            request.Search = request.Search?.ToLower().Trim() ?? "";
            var query = await _unitOfWork.LotRepository.GetAll()
                            .Include(x => x.Warehouse)
                            .Include(x => x.Provider)
                            .Where(x => x.LotNumber.Contains(request.Search) || x.WarehouseId.Equals(request.Search))
                            .ToPaginatedResultAsync(request.Page, request.PageSize);
            return query.Adapt<PaginatedResult<ViewLot>>();
        }


    }
}
