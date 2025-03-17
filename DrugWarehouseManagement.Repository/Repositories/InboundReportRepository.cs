using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class InboundReportRepository : GenericRepository<InboundReport>, IInboundReportRepository
    {
        public InboundReportRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
