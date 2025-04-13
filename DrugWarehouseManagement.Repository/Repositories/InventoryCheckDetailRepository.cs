using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class InventoryCheckDetailRepository : GenericRepository<InventoryCheckDetail>, IInventoryCheckDetailRepository
    {
        public InventoryCheckDetailRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
