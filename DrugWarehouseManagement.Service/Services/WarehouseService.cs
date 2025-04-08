using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace DrugWarehouseManagement.Service.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;
        public WarehouseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateWarehouseAsync(CreateWarehouseRequest request)
        {
            // Map the request to Warehouse entity
            var warehouse = request.Adapt<Warehouse>();
            await _unitOfWork.WarehouseRepository.CreateAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResult<WarehouseResponse>> SearchWarehousesAsync(SearchWarehouseRequest request)
        {
            var query = _unitOfWork.WarehouseRepository
                .GetAll()
                .Where(w => w.Status == WarehouseStatus.Active)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<WarehouseStatus>(request.Status, true, out var parsedStatus))
                {
                    query = query.Where(o => o.Status == parsedStatus);
                }
                else
                {
                    throw new Exception("Status is invalid.");
                }
            }
            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();
                query = query.Where(w =>
                    EF.Functions.Like(w.WarehouseName.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(w.Address.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(w.WarehouseCode.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(w.DocumentNumber.ToLower(), $"%{searchTerm}%")
                );
            }
            query = query.OrderByDescending(w => w.WarehouseId);

            var paginatedResult = await query.ToPaginatedResultAsync(request.Page, request.PageSize);

            var response = paginatedResult.Items.Adapt<List<WarehouseResponse>>();
            return new PaginatedResult<WarehouseResponse>
            {
                Items = response,
                TotalCount = paginatedResult.TotalCount,
                PageSize = paginatedResult.PageSize,
                CurrentPage = paginatedResult.CurrentPage
            };
        }

        public async Task UpdateWarehouseAsync(int warehouseId, UpdateWarehouseRequest request)
        {
            var warehouse = await _unitOfWork.WarehouseRepository
                                .GetAll()
                                .FirstOrDefaultAsync(w => w.WarehouseId == warehouseId);
            if (warehouse == null)
            {
                throw new Exception("Không tìm thấy kho.");
            }
            request.Adapt(warehouse);
            await _unitOfWork.WarehouseRepository.UpdateAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<BaseResponse> DeleteWarehouseAsync(int warehouseId)
        {
            var warehouse = await _unitOfWork.WarehouseRepository
                                .GetAll()
                                .FirstOrDefaultAsync(w => w.WarehouseId == warehouseId && w.Status == WarehouseStatus.Active);
            if (warehouse == null)
            {
                throw new Exception("Warehouse not found or already inactive.");
            }
            // Soft delete: update status to Inactive
            warehouse.Status = WarehouseStatus.Inactive;
            await _unitOfWork.WarehouseRepository.UpdateAsync(warehouse);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = (int)System.Net.HttpStatusCode.OK,
                Message = "Warehouse deleted successfully."
            };
        }
    }
}
