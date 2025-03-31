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
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class ReturnOutboundService : IReturnOutboundService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReturnOutboundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// Tạo các bản ghi ReturnOutboundDetails khi có hàng trả về
        /// </summary>
        /// 
        public async Task CreateReturnOutboundAsync(CreateReturnOutboundRequest request)
        {
            // 1) Kiểm tra Outbound có tồn tại, status = Completed hay chưa
            var outbound = await _unitOfWork.OutboundRepository
                .GetByWhere(o => o.OutboundId == request.OutboundId)
                .Include(o => o.OutboundDetails)
                .FirstOrDefaultAsync();

            if (outbound == null)
            {
                throw new Exception($"OutboundId={request.OutboundId} not found.");
            }

            if (outbound.Status != OutboundStatus.Completed)
            {
                throw new Exception("Chỉ được trả hàng khi Outbound ở trạng thái Completed.");
            }

            // 2) Lặp qua các dòng trả về
            var returnDetailsList = new List<ReturnOutboundDetails>();

            foreach (var detailItem in request.Details)
            {
                // Kiểm tra OutboundDetail
                var outboundDetail = outbound.OutboundDetails
                    .FirstOrDefault(od => od.OutboundDetailsId == detailItem.OutboundDetailId);

                if (outboundDetail == null)
                {
                    throw new Exception($"OutboundDetailId={detailItem.OutboundDetailId} not found in this outbound.");
                }

                // Kiểm tra InboundDetail
                var inboundDetail = await _unitOfWork.InboundDetailRepository
                    .GetByWhere(i => i.InboundDetailsId == detailItem.InboundDetailId)
                    .FirstOrDefaultAsync();

                if (inboundDetail == null)
                {
                    throw new Exception($"InboundDetailId={detailItem.InboundDetailId} not found.");
                }

                // Kiểm tra logic returnedQuantity <= outboundDetail.Quantity
                // (nếu bạn giới hạn không trả quá số đã xuất)
                if (detailItem.ReturnedQuantity > outboundDetail.Quantity)
                {
                    throw new Exception($"ReturnedQuantity={detailItem.ReturnedQuantity} > OutboundDetail.Quantity={outboundDetail.Quantity}");
                }

                // Tạo record ReturnOutboundDetails
                var rod = new ReturnOutboundDetails
                {
                    OutboundDetailsId = detailItem.OutboundDetailId,
                    InboundDetailId = detailItem.InboundDetailId,
                    ReturnedQuantity = detailItem.ReturnedQuantity,
                    Note = detailItem.Note              
                };
                rod.CreatedAt = SystemClock.Instance.GetCurrentInstant();
                returnDetailsList.Add(rod);
            }

            // 3) Lưu DB
            await _unitOfWork.ReturnOutboundDetailsRepository.CreateRangeAsync(returnDetailsList);
            await _unitOfWork.SaveChangesAsync();

            //  Cập nhật Outbound.Status = Returned 
            //  trả xong => status=Returned
             outbound.Status = OutboundStatus.Returned;
             await _unitOfWork.OutboundRepository.UpdateAsync(outbound);
             await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Lấy danh sách ReturnOutboundDetails dựa trên OutboundId
        /// </summary>
        public async Task<List<ReturnOutboundDetailsResponse>> GetReturnOutboundByOutboundIdAsync(int outboundId)
        {
            // Lấy tất cả OutboundDetail của Outbound
            var outboundDetails = await _unitOfWork.OutboundDetailsRepository
                .GetAll()
                .Include(od => od.Outbound)
                .Where(od => od.OutboundId == outboundId)
                .ToListAsync();

            var outboundDetailIds = outboundDetails.Select(od => od.OutboundDetailsId).ToList();

            // Lấy tất cả ReturnOutboundDetails có OutboundDetailId thuộc OutboundId này
            var returnOutboundDetails = await _unitOfWork.ReturnOutboundDetailsRepository
                .GetAll()
                .Include(r => r.OutboundDetails)
                    .ThenInclude(od => od.Outbound)
                 .Include(od => od.OutboundDetails.Lot)
                    .ThenInclude(l => l.Product)
                .Include(r => r.InboundDetail)
                    .ThenInclude(id => id.Inbound)
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
