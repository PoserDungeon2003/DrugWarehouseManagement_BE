using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class ReturnOutboundService : IReturnOutboundService
    {
        private const int TEMPORARY_WAREHOUSE_ID = 6;
        private readonly IUnitOfWork _unitOfWork;

        public ReturnOutboundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Tạo các bản ghi ReturnOutboundDetails và xử lý trả hàng
        /// </summary>
        public async Task CreateReturnOutboundAsync(CreateReturnOutboundRequest request)
        {
            // 1) Kiểm tra Outbound
            var outbound = await _unitOfWork.OutboundRepository
                .GetByWhere(o => o.OutboundId == request.OutboundId)
                .Include(o => o.OutboundDetails)
                    .ThenInclude(od => od.Lot)
                .FirstOrDefaultAsync();

            if (outbound == null)
            {
                throw new Exception($"OutboundId={request.OutboundId} not found.");
            }
            if (outbound.Status != OutboundStatus.Completed)
            {
                throw new Exception("Chỉ được trả hàng khi Outbound ở trạng thái Completed.");
            }

            // 2) Xử lý từng dòng trả hàng
            var returnDetailsList = new List<ReturnOutboundDetails>();
            // Nhóm các InboundDetails theo từng Provider: key = ProviderId, value = list InboundDetails
            var inboundDetailsByProvider = new Dictionary<int, List<InboundDetails>>();

            foreach (var detailItem in request.Details)
            {
                var outboundDetail = outbound.OutboundDetails
                    .FirstOrDefault(od => od.OutboundDetailsId == detailItem.OutboundDetailsId);

                if (outboundDetail == null)
                {
                    throw new Exception($"OutboundDetailId={detailItem.OutboundDetailsId} not found in this outbound.");
                }
                if (detailItem.Quantity > outboundDetail.Quantity)
                {
                    throw new Exception($"ReturnedQuantity={detailItem.Quantity} > OutboundDetail.Quantity={outboundDetail.Quantity}");
                }

                // Tạo bản ghi ReturnOutboundDetails
                var rod = new ReturnOutboundDetails
                {
                    OutboundDetailsId = detailItem.OutboundDetailsId,
                    ReturnedQuantity = detailItem.Quantity,
                    Note = detailItem.Note,
                    CreatedAt = SystemClock.Instance.GetCurrentInstant()
                };
                returnDetailsList.Add(rod);

                // Tạo đối tượng InboundDetails từ OutboundDetail
                var inboundDetail = new InboundDetails
                {
                    LotNumber = outboundDetail.Lot.LotNumber,
                    ManufacturingDate = outboundDetail.Lot.ManufacturingDate,
                    ExpiryDate = outboundDetail.Lot.ExpiryDate,
                    ProductId = outboundDetail.Lot.ProductId,
                    Quantity = detailItem.Quantity,
                    UnitPrice = outboundDetail.UnitPrice,
                    TotalPrice = outboundDetail.UnitPrice * detailItem.Quantity
                };

                // Nhóm theo Provider (lấy ProviderId từ Lot của OutboundDetail)
                int providerId = outboundDetail.Lot.ProviderId;
                if (!inboundDetailsByProvider.ContainsKey(providerId))
                {
                    inboundDetailsByProvider[providerId] = new List<InboundDetails>();
                }
                inboundDetailsByProvider[providerId].Add(inboundDetail);
            }

            // 3) Lưu ReturnOutboundDetails vào DB
            await _unitOfWork.ReturnOutboundDetailsRepository.AddRangeAsync(returnDetailsList);
            await _unitOfWork.SaveChangesAsync();

            // 4) Cập nhật Outbound.Status = Returned
            outbound.Status = OutboundStatus.Returned;
            outbound.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
            await _unitOfWork.OutboundRepository.UpdateAsync(outbound);
            await _unitOfWork.SaveChangesAsync();

            // 5) Tạo phiếu Inbound cho mỗi nhóm Provider và cập nhật lô (Lot) vào kho chỉ định
            foreach (var kvp in inboundDetailsByProvider)
            {
                int providerId = kvp.Key;
                List<InboundDetails> detailsForProvider = kvp.Value;
                var lotNumbers = string.Join(", ", detailsForProvider.Select(d => d.LotNumber));

                var newInbound = new Inbound
                {
                    InboundCode = GenerateInboundCode(),
                    Note = $"Tạo tự động từ hoàn trả đơn Outbound {outbound.OutboundCode} cho các lô hàng: {lotNumbers}",
                    InboundDate = SystemClock.Instance.GetCurrentInstant(),
                    Status = InboundStatus.Completed,
                    ProviderId = providerId,
                    AccountId = outbound.AccountId,
                    WarehouseId = TEMPORARY_WAREHOUSE_ID, // Kho chỉ định (tạm thời)
                    InboundRequestId = null, // Nhập trả hàng
                    InboundDetails = new List<InboundDetails>(),
                    CreatedAt = SystemClock.Instance.GetCurrentInstant(),
                };


                // Thêm danh sách InboundDetails tương ứng vào phiếu Inbound
                foreach (var detail in detailsForProvider)
                {
                    newInbound.InboundDetails.Add(detail);
                }

                await _unitOfWork.InboundRepository.CreateAsync(newInbound);

                // Cập nhật hoặc tạo mới lô (Lot) dựa trên Inbound và InboundDetails
                await CreateOrUpdateLotAsync(newInbound);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Tạo hoặc cập nhật lô (Lot) trong kho chỉ định dựa trên phiếu Inbound và các chi tiết InboundDetails
        /// Nếu đã có lô trùng LotNumber, ManufacturingDate, ExpiryDate và ProductId thì cộng thêm số lượng vào lô đó.
        /// </summary>
        private async Task CreateOrUpdateLotAsync(Inbound inbound)
        {
            foreach (var inboundDetail in inbound.InboundDetails)
            {
                var existingLot = await _unitOfWork.LotRepository
                    .GetByWhere(l =>
                        l.WarehouseId == inbound.WarehouseId &&
                        l.LotNumber == inboundDetail.LotNumber &&
                        l.ManufacturingDate == inboundDetail.ManufacturingDate &&
                        l.ExpiryDate == inboundDetail.ExpiryDate &&
                        l.ProductId == inboundDetail.ProductId)
                    .FirstOrDefaultAsync();

                if (existingLot != null)
                {
                    // Cộng thêm số lượng vào lô đã tồn tại
                    existingLot.Quantity += inboundDetail.Quantity;
                    await _unitOfWork.LotRepository.UpdateAsync(existingLot);
                }
                else
                {
                    // Tạo mới lô với thông tin từ InboundDetail
                    var newLot = new Lot
                    {
                        LotNumber = inboundDetail.LotNumber,
                        ManufacturingDate = inboundDetail.ManufacturingDate,
                        ExpiryDate = inboundDetail.ExpiryDate ?? DateOnly.FromDateTime(DateTime.Today),
                        ProductId = inboundDetail.ProductId,
                        WarehouseId = inbound.WarehouseId?? 6,
                        ProviderId = inbound.ProviderId,
                        Quantity = inboundDetail.Quantity
                    };
                    
                    await _unitOfWork.LotRepository.CreateAsync(newLot);
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        private string GenerateInboundCode()
        {
            return $"IN-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        public async Task<List<ReturnOutboundDetailsResponse>> GetReturnOutboundByOutboundIdAsync(int outboundId)
        {
            // Lấy tất cả OutboundDetail của Outbound
            var outboundDetails = await _unitOfWork.OutboundDetailsRepository
                .GetAll()
                .Include(od => od.Outbound)
                .Where(od => od.OutboundId == outboundId)
                .ToListAsync();

            var outboundDetailIds = outboundDetails.Select(od => od.OutboundDetailsId).ToList();

            // Lấy các ReturnOutboundDetails có OutboundDetailId thuộc Outbound này
            var returnOutboundDetails = await _unitOfWork.ReturnOutboundDetailsRepository
                .GetAll()
                .Include(r => r.OutboundDetails)
                    .ThenInclude(od => od.Outbound)
                .Include(r => r.OutboundDetails.Lot)
                    .ThenInclude(l => l.Product)
                .Where(r => outboundDetailIds.Contains(r.OutboundDetailsId))
                .ToListAsync();

            var response = returnOutboundDetails.Select(r =>
            {
                var dto = r.Adapt<ReturnOutboundDetailsResponse>();
                dto.OutboundDetailId = r.OutboundDetailsId;
                dto.OutboundCode = r.OutboundDetails.Outbound.OutboundCode;
                dto.ProductCode = r.OutboundDetails.Lot.Product.ProductCode;
                dto.ProductName = r.OutboundDetails.Lot.Product.ProductName;
                return dto;
            }).ToList();

            return response;
        }

        public async Task<List<ReturnOutboundDetailsResponse>> GetAllReturnOutboundDetailsAsync()
        {
            var returnOutboundDetails = await _unitOfWork.ReturnOutboundDetailsRepository
                .GetAll()
                .Include(r => r.OutboundDetails)
                    .ThenInclude(od => od.Outbound)
                .Include(r => r.OutboundDetails.Lot)
                    .ThenInclude(l => l.Product)
                .ToListAsync();

            var response = returnOutboundDetails.Select(r =>
            {
                var dto = r.Adapt<ReturnOutboundDetailsResponse>();
                dto.OutboundDetailId = r.OutboundDetailsId;
                dto.OutboundCode = r.OutboundDetails.Outbound.OutboundCode;
                dto.ProductCode = r.OutboundDetails.Lot.Product.ProductCode;
                dto.ProductName = r.OutboundDetails.Lot.Product.ProductName;
                return dto;
            }).ToList();

            return response;
        }
    }
}
