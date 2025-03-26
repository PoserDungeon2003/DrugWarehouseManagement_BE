using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
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
