using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface INotificationService
    {
        Task PushNotificationToAll(Notification notification);
        Task PushNotificationToRole(string role, Notification notification);
        Task<BaseResponse> ReadAllNotifications(string role);
        Task<PaginatedResult<ViewNotification>> GetNotificationsByRole(QueryPaging query, string role);
    }
}
