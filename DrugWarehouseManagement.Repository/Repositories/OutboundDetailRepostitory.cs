using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class OutboundDetailRepostitory : GenericRepository<OutboundDetails>, IOutboundDetailsRepository
    {
        public OutboundDetailRepostitory(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
