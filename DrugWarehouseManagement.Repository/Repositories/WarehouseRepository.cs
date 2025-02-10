using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class WarehouseRepository : GenericRepository<Warehouse>, IWarehouseRepository
    {
        public WarehouseRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
