using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface INotificationService
    {
        Task<int> GetUnreadNotificationsCount(Guid userId);
        Task<ViewNotification> PushNotificationToAll(Notification notification);
        Task<ViewNotification> PushNotificationToRole(string role, Notification notification);
        Task ReadAllNotifications(Guid userId);
    }
}
