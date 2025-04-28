using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Hubs
{
    public interface INotificationHub
    {
        Task ReceiveMessage(string user, string message);
    }
}
