using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class InboundRequestAssetsRepository : GenericRepository<InboundRequestAssets>, IInboundRequestAssetsRepository
    {
        public InboundRequestAssetsRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
