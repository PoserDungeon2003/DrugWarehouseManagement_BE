using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository
{
    public class InventoryTransactionRepository : GenericRepository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
      
}
