using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using FirebaseAdmin.Messaging;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using OpenCvSharp.Detail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class InboundRequestService : IInboundRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMinioService _minioService;
        private readonly string BucketName = "inboundrequest";
        private readonly INotificationService _notificationService;
        private readonly ILogger<InboundRequestService> _logger; 
        public InboundRequestService(IUnitOfWork unitOfWork, IMinioService minioService, ILogger<InboundRequestService> logger, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _minioService = minioService;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<BaseResponse> CreateInboundRequest(Guid accountId, CreateInboundOrderRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            var inboundRequestCode = GenerateInboundRequestCode();

            var inboundRequest = request.Adapt<InboundRequest>();
            inboundRequest.InboundRequestCode = inboundRequestCode;
            inboundRequest.AccountId = accountId;
            inboundRequest.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            inboundRequest.Assets = new List<Asset>();

            if (request.InboundRequestDetails != null && request.InboundRequestDetails.Any())
            {
                inboundRequest.Price = request.InboundRequestDetails.Sum(x => x.TotalPrice); // Set the Price field
            }
            else
            {
                inboundRequest.Price = 0; // Set Price to 0 if there are no details
            }

            // Handle image uploads if present
            if (request.Images != null && request.Images.Any())
            {
                try
                {
                    var uploadedAssets = await UploadFiles(request.Images, accountId);
                    inboundRequest.Assets.AddRange(uploadedAssets);
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

            // Send notification to relevant roles
            var noti = new Repository.Models.Notification
            {
                Title = "New Inbound Request",
                Content = "A new inbound request has been created.",
            };
            await _notificationService.PushNotificationToRole("Accountant" , noti);

            await _unitOfWork.InboundRequestRepository.CreateAsync(inboundRequest);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Inbound Request record created successfully",
            };
        }

        public async Task<ViewInboundRequest> GetInboundRequestById(int inboundRequestId)
        {
            var inboundRequest = await _unitOfWork.InboundRequestRepository
                .GetByWhere(i => i.InboundRequestId == inboundRequestId)
                .Include(i => i.InboundRequestDetails)
                .ThenInclude(i => i.Product)
                .AsQueryable()
                .FirstOrDefaultAsync();
            if (inboundRequest == null)
            {
                throw new Exception("Inbound Request not found");
            }

            var result = inboundRequest.Adapt<ViewInboundRequest>();
            // Manually map CreatedAt to CreateDate in the desired format
            result.CreateDate = inboundRequest.CreatedAt.ToString("dd/MM/yyyy HH:mm", null);
            return result;
        }

        public async Task<PaginatedResult<ViewInboundRequest>> GetInboundRequestsPaginatedAsync(InboundRequestQueryPaging request)
        {
            var query = _unitOfWork.InboundRequestRepository
                        .GetAll()
                        .Include(i => i.Assets)
                        .Include(i => i.InboundRequestDetails)
                        .ThenInclude(i => i.Product)
                        .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(i => i.InboundRequestCode.Contains(request.Search));
            }

            if (Enum.IsDefined(typeof(InboundRequestStatus), request.InboundRequestStatus))
            {
                query = query.Where(i => i.Status == request.InboundRequestStatus);
            }

            var pattern = InstantPattern.ExtendedIso;

            if (!string.IsNullOrEmpty(request.DateFrom))
            {
                var parseResult = pattern.Parse(request.DateFrom);
                if (parseResult.Success)
                {
                    Instant dateFromInstant = parseResult.Value;
                    query = query.Where(i => i.CreatedAt >= dateFromInstant);
                }
            }

            if (!string.IsNullOrEmpty(request.DateTo))
            {
                var parseResult = pattern.Parse(request.DateTo);
                if (parseResult.Success)
                {
                    Instant dateToInstant = parseResult.Value;
                    query = query.Where(i => i.CreatedAt <= dateToInstant);
                }
            }

            query = query.OrderByDescending(i => i.CreatedAt);

            // Paginate the result
            var paginatedInbounds = await query.ToPaginatedResultAsync(request.Page, request.PageSize);

            var viewInboundRequests = paginatedInbounds.Items.Adapt<List<ViewInboundRequest>>();

            foreach (var viewInbound in viewInboundRequests)
            {
                if (viewInbound.CreateDate != null)
                {
                    viewInbound.CreateDate = InstantPattern.ExtendedIso.Parse(viewInbound.CreateDate)
                        .Value.ToString("dd/MM/yyyy HH:mm", null);
                }
            }
            return new PaginatedResult<ViewInboundRequest>
            {
                Items = viewInboundRequests,
                TotalCount = paginatedInbounds.TotalCount,
                PageSize = paginatedInbounds.PageSize,
                CurrentPage = paginatedInbounds.CurrentPage
            };
        }

        public async Task<BaseResponse> UpdateInboundRequest(Guid accountId, UpdateInboundOrderRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            // Validate if the inbound exists
            var inboundRequest = await _unitOfWork.InboundRequestRepository
                .GetByWhere(ir => ir.InboundRequestId == request.InboundOrderId)
                .Include(ir => ir.Assets)
                .FirstOrDefaultAsync();
            if (inboundRequest == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound Request not found" };
            }

            if (inboundRequest.Status == InboundRequestStatus.Completed || inboundRequest.Status == InboundRequestStatus.Cancelled)
            {
                return new BaseResponse { Code = 200, Message = "Inbound Request is Completed or Cancelled that can not update" };
            }

            inboundRequest.AccountId = accountId;
            inboundRequest.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            request.Adapt(inboundRequest);

            if (request.InboundRequestDetails != null && request.InboundRequestDetails.Any())
            {
                // Remove all existing details
                var existingDetails = _unitOfWork.InboundRequestDetailsRepository.GetByWhere(x => x.InboundRequestId == inboundRequest.InboundRequestId);
                await _unitOfWork.InboundRequestDetailsRepository.DeleteRangeAsync(existingDetails);

                inboundRequest.Price = inboundRequest.InboundRequestDetails.Sum(x => x.TotalPrice); // Set the Price field
            }
            else
            {
                inboundRequest.Price = 0; // Set Price to 0 if there are no details
            }

            if (request.Images != null && request.Images.Any())
            {
                try
                {
                    // Remove all existing assets associated with this inbound report
                    var existingAssets = _unitOfWork.InboundRequestAssetsRepository.GetByWhere(x => x.InboundRequestId == inboundRequest.InboundRequestId);
                    await _unitOfWork.InboundRequestAssetsRepository.DeleteRangeAsync(existingAssets);

                    // Upload new files and associate them with the inbound report
                    var uploadedAssets = await UploadFiles(request.Images, accountId);
                    inboundRequest.Assets.AddRange(uploadedAssets);
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

            await _unitOfWork.InboundRequestRepository.UpdateAsync(inboundRequest);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound Request updated successfully" };
        }

        public async Task<BaseResponse> UpdateInboundRequestStatus(Guid accountId, UpdateInboundOrderStatusRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            var inboundRequest = await _unitOfWork.InboundRequestRepository.GetByIdAsync(request.InboundId);
            if (inboundRequest == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound Request not found" };
            }

            if (!Enum.IsDefined(typeof(InboundRequestStatus), request.InboundOrderStatus))
            {
                return new BaseResponse { Code = 404, Message = "Invalid inbound request status {WaitingForAccountantApproval, WaitingForDirectorApproval, WaitingForSaleAdminApproval, InProgress, WaitingForImport, Completed, Cancelled}" };
            }

            // Update inbound request
            inboundRequest.Status = request.InboundOrderStatus;
            inboundRequest.AccountId = accountId;
            inboundRequest.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            await _unitOfWork.InboundRequestRepository.UpdateAsync(inboundRequest);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound Request updated status successfully" };
        }

        private string GenerateInboundRequestCode()
        {
            var dateDigits = DateTime.Now.ToString("MMdd");
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            return $"IRC{uniqueId}{dateDigits}";
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
                        AccountId = accountId,
                        ContentType = file.ContentType,
                    };

                    uploadedAssets.Add(asset);
                }
            }

            return uploadedAssets;
        }
    }
}
