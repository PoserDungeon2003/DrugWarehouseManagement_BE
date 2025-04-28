using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Hubs;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        // Đếm số thông báo chưa đọc của một người dùng
        public async Task<int> GetUnreadNotificationsCount(Guid accountId)
        {
            try
            {
                var notifications = await _unitOfWork.NotificationRepository.GetAll().ToListAsync();
                return notifications.Count(n => n.AccountId == accountId && !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError($"500 - Error fetching unread notifications: {ex.Message}");
                throw;
            }
        }

        public async Task<ViewNotification> PushNotificationToAll(Notification notification)
        {
            try
            {
                var noti = notification.Adapt<ViewNotification>();

                await _unitOfWork.NotificationRepository.CreateAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                await _notificationHubContext.Clients.All.SendAsync("ReceiveMessage");

                _logger.LogInformation($"Notification sent to all users: {notification.Title}");
                return noti;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error pushing notification to all users: {ex.Message}");
                throw;
            }
        }

        // Push notification to a specific role
        public async Task<ViewNotification> PushNotificationToRole(string role, Notification notification)
        {
            try
            {
                var noti = notification.Adapt<ViewNotification>();

                await _unitOfWork.NotificationRepository.CreateAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Notification sent to role {role}: {notification.Title}");

                await _notificationHubContext.Clients.Group(role.ToString()).SendAsync("ReceiveMessage", noti);
                return noti;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error pushing notification to role {role}: {ex.Message}");
                throw;
            }
        }

        // Đánh dấu tất cả thông báo của một người dùng là đã đọc
        public async Task ReadAllNotifications(Guid userId)
        {
            try
            {
                var notifications = await _unitOfWork.NotificationRepository.GetAll().ToListAsync();
                var userNotifications = notifications.Where(n => n.AccountId == userId && !n.IsRead).ToList();

                if (!userNotifications.Any())
                {
                    _logger.LogInformation($"No unread notifications for user {userId}.");
                    return;
                }

                userNotifications.ForEach(n => n.IsRead = true);
                await _unitOfWork.NotificationRepository.AddRangeAsync(userNotifications);

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"All notifications for user {userId} marked as read.");
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
    }
}
