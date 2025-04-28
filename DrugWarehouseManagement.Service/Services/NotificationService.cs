using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Hubs;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime.Text;
using NodaTime;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class NotificationService : INotificationService
    { 
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly ILogger<NotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public NotificationService(IHubContext<NotificationHub> notificationHubContext, ILogger<NotificationService> logger, IUnitOfWork unitOfWork)
        {
            _notificationHubContext = notificationHubContext;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task PushNotificationToAll(Notification notification)
        {
            try
            {
                var noti = notification.Adapt<ViewNotification>();

                await _unitOfWork.NotificationRepository.CreateAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                await _notificationHubContext.Clients.All.SendAsync("ReceiveMessage");

                _logger.LogInformation($"Notification sent to all users: {notification.Title}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error pushing notification to all users: {ex.Message}");
                throw;
            }
        }

        // Push notification to a specific role
        public async Task PushNotificationToRole(string role, Notification notification)
        {
            try
            {
                var noti = notification.Adapt<ViewNotification>();

                await _unitOfWork.NotificationRepository.CreateAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Notification sent to role {role}: {notification.Title}");

                await _notificationHubContext.Clients.Group(role.ToString()).SendAsync("ReceiveMessage", noti);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error pushing notification to role {role}: {ex.Message}");
                throw;
            }
        }

        // Đánh dấu tất cả thông báo của một người dùng là đã đọc
        public async Task ReadAllNotifications(string role)
        {
            try
            {
                var notis = await _unitOfWork.NotificationRepository.GetByWhere(n => n.Role == role && n.IsRead == false).ToListAsync();


                notis.ForEach(n => n.IsRead = true);
                await _unitOfWork.NotificationRepository.AddRangeAsync(notis);

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"All notifications for role {role} marked as read.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"500 - Error marking notifications as read: {ex.Message}");
                throw;
            }
        }

        // Đọc một thông báo cụ thể
        public async Task<ViewNotification> ReadNotification(Guid notificationId)
        {
            try
            {
                var notification = await _unitOfWork.NotificationRepository.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    throw new KeyNotFoundException("Notification not found.");
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    await _unitOfWork.NotificationRepository.UpdateAsync(notification);
                    await _unitOfWork.SaveChangesAsync();
                }

                _logger.LogInformation($"Notification with ID {notificationId} marked as read.");
                return notification.Adapt<ViewNotification>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"500 - Error reading notification: {ex.Message}");
                throw;
            }
        }

        public async Task<PaginatedResult<ViewNotification>> GetNotificationsByRole(QueryPaging request)
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

            //if (Enum.IsDefined(typeof(InboundRequestStatus), request.InboundRequestStatus))
            //{
            //    query = query.Where(i => i.Status == request.InboundRequestStatus);
            //}

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

            var viewInboundRequests = paginatedInbounds.Items.Adapt<List<ViewNotification>>();

            return new PaginatedResult<ViewNotification>
            {
                Items = viewInboundRequests,
                TotalCount = paginatedInbounds.TotalCount,
                PageSize = paginatedInbounds.PageSize,
                CurrentPage = paginatedInbounds.CurrentPage
            };
        }
    }
}
