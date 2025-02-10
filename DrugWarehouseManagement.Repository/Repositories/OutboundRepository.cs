using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class OutboundRepository : GenericRepository<Outbound>, IOutboundRepository
    {
        public OutboundRepository(DrugWarehouseContext context) : base(context)
        {
        }

    }
}
