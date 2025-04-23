using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly string[] _relevantRoles = { "Sale Admin", "Inventory Manager", "Accountant", "Director" };
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var role = Context.User.FindFirstValue(ClaimTypes.Role);
            var connectionId = Context.ConnectionId;

            if (_relevantRoles.Contains(role))
            {
                await Groups.AddToGroupAsync(connectionId, $"{role}s");
                _logger.LogInformation("Connection established: {ConnectionId}, Role: {Role}", connectionId, role);
            }
            else
            {
                _logger.LogInformation("Connection established without matching role: {ConnectionId}", connectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var role = Context.User.FindFirstValue(ClaimTypes.Role);
            var connectionId = Context.ConnectionId;

            if (_relevantRoles.Contains(role))
            {
                await Groups.RemoveFromGroupAsync(connectionId, $"{role}s");
                _logger.LogInformation("Connection disconnected: {ConnectionId}, Role: {Role}", connectionId, role);
            }
            else
            {
                _logger.LogInformation("Connection disconnected without matching role: {ConnectionId}", connectionId);
            }

            if (exception != null)
            {
                _logger.LogError(exception, "Disconnection caused by exception");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

