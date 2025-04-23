using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface INotificationService
    {
        Task NotifyRelevantRolesAsync(string message);
        Task NotifyRoleAsync(string role, string message);
    }
}
