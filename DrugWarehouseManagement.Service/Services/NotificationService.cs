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

                await _notificationHubContext.Clients.Group($"{role.ToString()}s").SendAsync("ReceiveMessage", noti);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error pushing notification to role {role}: {ex.Message}");
                throw;
            }
        }

        // Đánh dấu tất cả thông báo của một người dùng là đã đọc
        public async Task<BaseResponse> ReadAllNotifications(string role)
        {
            try
            {
                var notis = await _unitOfWork.NotificationRepository.GetByWhere(n => n.Role == role && n.IsRead == false).ToListAsync();


                foreach (var noti in notis)
                {
                    noti.IsRead = true;
                    await _unitOfWork.NotificationRepository.UpdateAsync(noti);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"All notifications for role {role} marked as read.");
                return new BaseResponse
                {
                    Code = 200,
                    Message = "All notifications marked as read successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"500 - Error marking notifications as read: {ex.Message}");
                return new BaseResponse
                {
                    Code = 500,
                    Message = "Internal server error"
                };
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

        public async Task<PaginatedResult<ViewNotification>> GetNotificationsByRole(QueryPaging request, string role)
        {
            var query = _unitOfWork.NotificationRepository
                        .GetAll()
                        .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(i => i.Title.Contains(request.Search));
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

            query = query.Where(i => i.Role == role);
            query = query.OrderByDescending(x => x.IsRead == false).ThenByDescending(i => i.CreatedAt);

            // Paginate the result
            var paginatedNotifications = await query.ToPaginatedResultAsync(request.Page, request.PageSize);

            var viewNotifications= paginatedNotifications.Items.Adapt<List<ViewNotification>>();

            return new PaginatedResult<ViewNotification>
            {
                Items = viewNotifications,
                TotalCount = paginatedNotifications.TotalCount,
                PageSize = paginatedNotifications.PageSize,
                CurrentPage = paginatedNotifications.CurrentPage
            };
        }
    }
}
