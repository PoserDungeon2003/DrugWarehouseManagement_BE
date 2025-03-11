using DrugWarehouseManagement.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IInventoryService
    {
        Task<List<LotAlert>> CheckLowStockAndExpiryAsync();
        Task NotifyLowStockAndExpiryAsync();
    }
}
