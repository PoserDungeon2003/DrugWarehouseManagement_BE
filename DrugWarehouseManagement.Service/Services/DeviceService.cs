using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeviceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResult<ViewDevices>> GetDevices(QueryPaging queryPaging)
        {
            queryPaging.Search = queryPaging.Search?.ToLower().Trim() ?? "";
            var query = _unitOfWork.DeviceRepository.GetAll()
                        .Include(x => x.Account)
                        .OrderByDescending(x => x.UpdatedAt.HasValue)
                        .ThenByDescending(x => x.UpdatedAt)
                        .ThenByDescending(x => x.CreatedAt)
                        .Where(x => x.DeviceId.ToString().Contains(queryPaging.Search) || x.DeviceName.Contains(queryPaging.Search) || x.DeviceType.Contains(queryPaging.Search))
                        .AsQueryable();

            if (queryPaging.DateFrom != null)
            {
                var dateFrom = InstantPattern.ExtendedIso.Parse(queryPaging.DateFrom);
                if (!dateFrom.Success)
                {
                    throw new Exception("DateFrom is invalid ISO format");
                }
                query = query.Where(o => o.CreatedAt >= dateFrom.Value);
            }
            if (queryPaging.DateTo != null)
            {
                var dateTo = InstantPattern.ExtendedIso.Parse(queryPaging.DateTo);
                if (!dateTo.Success)
                {
                    throw new Exception("DateTo is invalid ISO format");
                }
                query = query.Where(o => o.CreatedAt <= dateTo.Value);
            }

            var result = await query.ToPaginatedResultAsync(queryPaging.Page, queryPaging.PageSize);
            return result.Adapt<PaginatedResult<ViewDevices>>();
        }

        public async Task<BaseResponse> Ping(string apiKey)
        {
            var device = await _unitOfWork.DeviceRepository
                .GetByWhere(d => d.ApiKey == apiKey)
                .FirstOrDefaultAsync();

            if (device == null)
            {
                throw new Exception("Device not found");
            }

            return new BaseResponse
            {
                Code = 200,
                Message = "Device is online"
            };
        }

        public async Task<BaseResponse> RegisterDevice(Guid accountId, RegisterDeviceRequest request)
        {
            var device = request.Adapt<Device>();

            device.ApiKey = Utils.GenerateApiKey();
            device.AccountId = accountId;

            await _unitOfWork.DeviceRepository.CreateAsync(device);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Device registered successfully",
                Result = new
                {
                    device.ApiKey
                }
            };
        }

        public async Task<BaseResponse> UpdateDevice(Guid accountId, UpdateDeviceRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }
            
            var device = await _unitOfWork.DeviceRepository
                  .GetByIdAsync(request.DeviceId);

            if (device == null)
            {
                throw new Exception("Device not found");
            }

            request.Adapt(device);
            device.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            await _unitOfWork.DeviceRepository.UpdateAsync(device);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Device updated successfully"
            };
        }

        public async Task<BaseResponse> UpdateTrackingNumber(string apiKey, UpdateTrackingNumberRequest request)
        {
            var device = await _unitOfWork.DeviceRepository
                .GetByWhere(d => d.ApiKey == apiKey)
                .FirstOrDefaultAsync();

            if (device == null)
            {
                throw new Exception("Device not found");
            }

            var outbound = await _unitOfWork.OutboundRepository
                .GetByWhere(o => o.OutboundCode == request.OutboundCode)
                .FirstOrDefaultAsync();

            if (outbound == null)
            {
                throw new Exception("Outbound not found");
            }

            outbound.TrackingNumber = request.TrackingNumber;

            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = 200,
                Message = "Tracking number updated successfully"
            };
        }
    }
}
