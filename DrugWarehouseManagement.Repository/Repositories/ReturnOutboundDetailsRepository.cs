using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class ReturnOutboundDetailsRepository : GenericRepository<ReturnOutboundDetails>, IReturnOutboundDetailsRepository   
    {
        public ReturnOutboundDetailsRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
