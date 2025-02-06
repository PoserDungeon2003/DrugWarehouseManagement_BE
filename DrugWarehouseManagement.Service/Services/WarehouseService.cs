using DrugWarehouseManagement.Common.Enums;
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

        public async Task<PaginatedResult<WarehouseResponse>> SearchWarehousesAsync(QueryPaging queryPaging)
        {
            // Only include warehouses with Active status
            var query = _unitOfWork.WarehouseRepository
                        .GetAll()
                        .AsQueryable();

            // If a search term is provided, filter by WarehouseName or Address
            if (!string.IsNullOrEmpty(queryPaging.Search))
            {
                var searchTerm = queryPaging.Search.Trim().ToLower();
                query = query.Where(w =>
                    EF.Functions.Like(w.WarehouseName.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(w.Address.ToLower(), $"%{searchTerm}%")
                );
            }

            // Optionally, add additional filtering by dates if the Warehouse model had such fields.
            // Order by WarehouseId descending (or any other order)
            query = query.OrderByDescending(w => w.WarehouseId);

            var paginatedResult = await query.ToPaginatedResultAsync(queryPaging.Page, queryPaging.PageSize);

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
                                .FirstOrDefaultAsync(w => w.WarehouseId == warehouseId && w.Status == WarehouseStatus.Active);
            if (warehouse == null)
            {
                throw new Exception("Warehouse not found or is inactive.");
            }
            warehouse.WarehouseName = request.WarehouseName;
            warehouse.Address = request.Address;

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
