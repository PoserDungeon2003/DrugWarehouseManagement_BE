using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class LotRepository : GenericRepository<Lot>, ILotRepository
    {
        public LotRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
