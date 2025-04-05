using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;
using NodaTime;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            var lot = request.Adapt<Lot>();

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

            lot = request.Adapt<Lot>();

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

            var result = lot.Adapt<ViewLot>();  

            return result;
        }

        public async Task<PaginatedResult<ViewLot>> GetLotsPaginatedAsync(LotQueryPaging request)
        {
            request.Search = request.Search?.Trim().ToLower() ?? "";
            var query = _unitOfWork.LotRepository
                        .GetAll()
                        .Include(x => x.Warehouse)
                        .Include(x => x.Provider)
                        .Include(x => x.Product)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(x =>
                    EF.Functions.Like(x.LotNumber.ToLower(), $"%{request.Search}%") ||
                    EF.Functions.Like(x.LotId.ToString(), $"%{request.Search}%") ||
                    EF.Functions.Like(x.Product.ProductName, $"%{request.Search}%"));
            }

            if (request.ProductId != 0)
            {
                query = query.Where(x => x.ProductId == request.ProductId);
            }

            if (request.ProviderId != 0)
            {
                query = query.Where(x => x.ProviderId == request.ProviderId);
            }

            if (request.WarehouseId != 0)
            {
                query = query.Where(x => x.WarehouseId == request.WarehouseId);
            }

            if (request.Availablle)
            {
                query = query.Where(x => x.Quantity > 0);
            }

            var pattern = InstantPattern.ExtendedIso;
            DateOnly? dateFrom = null, dateTo = null;

            if (!string.IsNullOrEmpty(request.DateFrom))
            {
                var parseResult = pattern.Parse(request.DateFrom);
                if (parseResult.Success)
                {
                    dateFrom = DateOnly.FromDateTime(parseResult.Value.ToDateTimeUtc());
                    query = query.Where(x => x.ExpiryDate >= dateFrom);
                }
            }

            if (!string.IsNullOrEmpty(request.DateTo))
            {
                var parseResult = pattern.Parse(request.DateTo);
                if (parseResult.Success)
                {
                    dateTo = DateOnly.FromDateTime(parseResult.Value.ToDateTimeUtc());
                    query = query.Where(x => x.ExpiryDate <= dateTo);
                }
            }

            // Sắp xếp theo ngày tạo mới nhất
            query = query.OrderByDescending(x => x.LotId);

            // Phân trang kết quả
            var paginatedLots = await query.ToPaginatedResultAsync(request.Page, request.PageSize);

            var viewLots = paginatedLots.Items.Adapt<List<ViewLot>>();
            return new PaginatedResult<ViewLot>
            {
                Items = viewLots,
                TotalCount = paginatedLots.TotalCount,
                PageSize = paginatedLots.PageSize,
                CurrentPage = paginatedLots.CurrentPage
            };
        }


    }
}
