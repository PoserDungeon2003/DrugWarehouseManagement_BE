using DrugWarehouseManagement.Common.Enums;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using Microsoft.EntityFrameworkCore;

namespace DrugWarehouseManagement.Service.Services
{
    public class InboundService : IInboundService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InboundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> CreateInbound(CreateInboundRequest request)
        {
            //// Validate if the account exists
            //var account = await _unitOfWork.AccountRepository.GetByIdAsync(request.AccountId);
            //if (account == null)
            //{
            //    throw new Exception("Account does not exist");
            //}

            //// Validate if the provider exists
            //var provider = await _unitOfWork.ProviderRepository.GetByIdAsync(request.ProviderId);
            //if (provider == null)
            //{
            //    throw new Exception("Provider does not exist");
            //}

            // Create Inbound entity (without product details)
            var inbound = new Inbound
            {
                InboundCode = request.InboundCode.Trim(),
                ProviderOrderCode = request.ProviderOrderCode.Trim(),
                Note = request.Note,
                ProviderId = request.ProviderId,
                AccountId = request.AccountId,
                UpdatedAt = SystemClock.Instance.GetCurrentInstant()
            };

            // Save inbound first to get the ID
            await _unitOfWork.InboundRepository.CreateAsync(inbound);
            await _unitOfWork.SaveChangesAsync();

            // Validate and create inbound details
            foreach (var detail in request.InboundDetailList)
            {
                //var product = await _unitOfWork.ProductRepository.GetByIdAsync(detail.ProductId);
                //if (product == null)
                //{
                //    throw new Exception($"Product with ID {detail.ProductId} does not exist");
                //}

                var inboundDetail = new InboundDetails
                {
                    InboundId = inbound.InboundId,  // Link to the created inbound
                    ProductId = detail.ProductId,
                    LotNumber = detail.LotNumber.Trim(),
                    ExpiryDate = detail.ExpiryDate,
                    Quantity = detail.Quantity,
                    UnitType = detail.UnitType.Trim(),
                    UnitPrice = detail.UnitPrice,
                    TotalPrice = detail.UnitPrice * detail.Quantity,
                    Status = InboundDetailStatus.Pending
                };

                await _unitOfWork.InboundDetailRepository.CreateAsync(inboundDetail);
            }

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Inbound record created successfully",
            };
        }

        public async Task<BaseResponse> UpdateInbound(UpdateInboundRequest request)
        {
            // Validate if the inbound exists
            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(request.InboundId);
            if (inbound == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            //// Validate if the provider exists
            //var provider = await _unitOfWork.ProviderRepository.GetByIdAsync(request.ProviderId);
            //if (provider == null)
            //{
            //    return new BaseResponse { Code = 404, Message = "Provider does not exist" };
            //}

            // Update inbound details
            inbound.ProviderOrderCode = string.IsNullOrWhiteSpace(request.ProviderOrderCode) ? inbound.ProviderOrderCode : request.ProviderOrderCode;
            inbound.InboundCode = string.IsNullOrWhiteSpace(request.InboundCode) ? inbound.InboundCode : request.InboundCode;
            inbound.Note = string.IsNullOrWhiteSpace(request.Note) ? inbound.Note : request.Note;
            if (request.InboundStatus.HasValue && Enum.IsDefined(typeof(InboundStatus), request.InboundStatus.Value))
            {
                inbound.Status = request.InboundStatus.Value;
            }
            inbound.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound updated successfully" };
        }

        public async Task<BaseResponse> DeleteInbound(int inboundId)
        {
            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(inboundId);
            if (inbound == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            // Remove all related inbound details first
            var inboundDetails = await _unitOfWork.InboundDetailRepository.GetAllByInboundIdAsync(inboundId);
            foreach (var detail in inboundDetails)
            {
                detail.Status = InboundDetailStatus.Rejected;
                await _unitOfWork.InboundDetailRepository.UpdateAsync(detail);
            }

            inbound.Status = InboundStatus.Cancelled;
            // Delete inbound record
            await _unitOfWork.InboundRepository.UpdateAsync(inbound);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound deleted successfully" };
        }

        public async Task<ViewInbound> GetInboundById(int inboundId)
        {
            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(inboundId);
            if (inbound == null)
            {
                throw new Exception("Inbound not found");
            }

            var inboundDetails = await _unitOfWork.InboundDetailRepository.GetAllByInboundIdAsync(inboundId);

            return new ViewInbound
            {
                InboundId = inbound.InboundId,
                InboundCode = inbound.InboundCode,
                ProviderOrderCode = inbound.ProviderOrderCode,
                Note = inbound.Note,
                ProviderName = inbound.Provider.ProviderName,
                InboundDetailList = inboundDetails.Select(detail => new InboundDetailResponse
                {
                    ProductName = detail.Products.ProductName,
                    LotNumber = detail.LotNumber,
                    ExpiryDate = detail.ExpiryDate,
                    Quantity = detail.Quantity,
                    ActualQuantity = detail.ActualQuantity,
                    UnitType = detail.UnitType,
                    UnitPrice = detail.UnitPrice,
                    TotalPrice = detail.TotalPrice
                }).ToList()
            };
        }

        public async Task<PaginatedResult<ViewInbound>> GetInboundsPaginatedAsync(QueryPaging request)
        {
            var query = _unitOfWork.InboundRepository
                        .GetAll()
                        .Include(i => i.InboundDetails)
                        .ThenInclude(id => id.Products)  // Assuming Products are related to InboundDetails
                        .AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();

                if (int.TryParse(searchTerm, out int inboundId))
                {
                    query = query.Where(i =>
                        i.InboundId == inboundId ||
                        EF.Functions.Like(i.InboundCode.ToLower(), $"%{searchTerm}%"));
                }
                else
                {
                    query = query.Where(i =>
                        EF.Functions.Like(i.InboundCode.ToLower(), $"%{searchTerm}%"));
                }
            }

            if (request.DateFrom.HasValue)
            {
                query = query.Where(i => i.CreatedAt >= request.DateFrom.Value);
            }
            if (request.DateTo.HasValue)
            {
                query = query.Where(i => i.CreatedAt <= request.DateTo.Value);
            }

            query = query.OrderByDescending(i => i.CreatedAt);

            // Paginate the result
            var paginatedInbounds = await query.ToPaginatedResultAsync(request.Page, request.PageSize);

            var viewInbounds = paginatedInbounds.Items.Select(inbound => new ViewInbound
            {
                InboundId = inbound.InboundId,
                InboundCode = inbound.InboundCode,
                ProviderOrderCode = inbound.ProviderOrderCode,
                Note = inbound.Note,
                ProviderName = inbound.Provider.ProviderName, // Assuming Provider is a related entity
                InboundDetailList = inbound.InboundDetails.Select(detail => new InboundDetailResponse
                {
                    ProductName = detail.Products.ProductName, // Assuming Products is a related entity
                    LotNumber = detail.LotNumber,
                    ExpiryDate = detail.ExpiryDate,
                    Quantity = detail.Quantity,
                    ActualQuantity = detail.ActualQuantity,
                    UnitType = detail.UnitType,
                    UnitPrice = detail.UnitPrice,
                    TotalPrice = detail.TotalPrice
                }).ToList()
            }).ToList();

            return new PaginatedResult<ViewInbound>
            {
                Items = viewInbounds,
                TotalCount = paginatedInbounds.TotalCount,
                PageSize = paginatedInbounds.PageSize,
                CurrentPage = paginatedInbounds.CurrentPage
            };
        }

    }
}
