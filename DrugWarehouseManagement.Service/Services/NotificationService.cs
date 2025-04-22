using DrugWarehouseManagement.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class NotificationService
    { 
        private readonly IHubContext<NotificationHub> _notificationHubContext; 
        public NotificationService(IHubContext<NotificationHub> notificationHubContext)
        {
            _notificationHubContext = notificationHubContext;
        }

        public async Task NotifyRelevantRolesAsync(string message)
        {
            string[] roles = { "Sale Admin", "Inventory Manager", "Accountant", "Director" };
            foreach (var role in roles)
            {
                await _notificationHubContext.Clients.Group($"{role}s").SendAsync("ReceiveNotification", message);
            }
        }

        public async Task NotifyRoleAsync(string role, string message)
        {
            await _notificationHubContext.Clients.Group($"{role}s").SendAsync("ReceiveNotification", message);
        }
    }
}
