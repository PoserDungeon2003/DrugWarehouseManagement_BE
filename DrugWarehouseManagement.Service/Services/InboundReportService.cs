using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class InboundReportService : IInboundReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMinioService _minioService;
        private readonly IInboundService _inboundService;
        private readonly string BucketName = "inboundreport";
        public InboundReportService(IUnitOfWork unitOfWork, IMinioService minioService, IInboundService inboundService)
        {
            _unitOfWork = unitOfWork;
            _minioService = minioService;
            _inboundService = inboundService;
        }
        public async Task<BaseResponse> CreateInboundReport(Guid accountId, CreateInboundReportRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            var inboundReport = request.Adapt<InboundReport>();
            inboundReport.AccountId = accountId;
            inboundReport.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
            inboundReport.Assets = new List<Asset>();

            // Handle image uploads if present
            if (request.Images != null && request.Images.Any())
            {
                try
                {
                    var uploadedAssets = await UploadFiles(request.Images, accountId);
                    inboundReport.Assets.AddRange(uploadedAssets);
                }
                catch (Exception ex)
                {
                    return new BaseResponse
                    {
                        Code = 500,
                        Message = "Error uploading files: " + ex.Message
                    };
                }
            }

            await _unitOfWork.InboundReportRepository.CreateAsync(inboundReport);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Inbound Request record created successfully",
            };
        }

        public async Task<List<ViewInboundReport>> GetInboundReportByInboundId(int inboundId)
        {
            var inboundReport = await _unitOfWork.InboundReportRepository
                .GetByWhere(ir => ir.InboundId == inboundId).Include(ir => ir.Assets).ToListAsync();

            if (inboundReport == null)
            {
                return new List<ViewInboundReport>();
            }

            // Assuming ViewInboundReport is a DTO/ViewModel that maps from InboundReport
            var result = inboundReport.Adapt<List<ViewInboundReport>>();
            return result ?? new List<ViewInboundReport>();
        }

        public async Task<BaseResponse> UpdateInboundReport(Guid accountId, UpdateInboundReportRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            var inboundReport = await _unitOfWork.InboundReportRepository
                .GetByWhere(ir => ir.InboundReportId == request.InboundReportId)
                .Include(ir => ir.Assets)
                .FirstOrDefaultAsync();

            if (inboundReport == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound report not found" };
            }

            if (inboundReport.Status == InboundReportStatus.Completed)
            {
                return new BaseResponse { Code = 200, Message = "Inbound report is completed and can't be update" };
            }

            request.Adapt(inboundReport);
            inboundReport.Status = InboundReportStatus.Completed;
            inboundReport.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            if (request.InboundReportStatus == InboundReportStatus.Completed)
            {
                await _inboundService.UpdateInboundStatus(accountId, new UpdateInboundStatusRequest { InboundId = inboundReport.InboundId, InboundStatus = InboundStatus.Completed });
            }

            // Handle image uploads if present
            if (request.Images != null && request.Images.Any())
            {
                try
                {
                    // Remove all existing assets associated with this inbound report
                    var existingAssets = _unitOfWork.InboundReportAssetsRepository.GetByWhere(x => x.InboundReportId == inboundReport.InboundReportId);
                    foreach (var asset in existingAssets)
                    {
                        await _unitOfWork.InboundReportAssetsRepository.DeleteAsync(asset);
                    }

                    var uploadedAssets = await UploadFiles(request.Images, accountId);
                    inboundReport.Assets.AddRange(uploadedAssets);
                }
                catch (Exception ex)
                {
                    return new BaseResponse
                    {
                        Code = 500,
                        Message = "Error uploading files: " + ex.Message
                    };
                }
            }


            await _unitOfWork.InboundReportRepository.UpdateAsync(inboundReport);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Inbound report updated successfully"
            };
        }

        private async Task<List<Asset>> UploadFiles(List<IFormFile> files, Guid accountId)
        {
            var uploadedAssets = new List<Asset>();
            // Upload each file using MinioService
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var uploadResponse = await _minioService.FileUpload(BucketName, file, fileName);

                    var asset = new Asset
                    {
                        FileUrl = $"{BucketName}/{fileName}",
                        FileName = fileName,
                        FileExtension = uploadResponse.Extension,
                        FileSize = file.Length,
                        UploadedAt = SystemClock.Instance.GetCurrentInstant(),
                        Status = AssetStatus.Active,
                        AccountId = accountId
                    };

                    uploadedAssets.Add(asset);
                }
            }

            return uploadedAssets;
        }
    }
}
