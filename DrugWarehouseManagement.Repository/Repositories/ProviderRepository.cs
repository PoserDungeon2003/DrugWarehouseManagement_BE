using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class ProviderRepository : GenericRepository<Provider>, IProviderRepository
    {
        public ProviderRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
