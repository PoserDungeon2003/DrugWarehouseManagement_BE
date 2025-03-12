using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class InboundDetailRepository : GenericRepository<InboundDetails>, IInboundDetailRepository
    {
        public InboundDetailRepository(DrugWarehouseContext context) : base(context)
        {

        }

        public async Task<IEnumerable<InboundDetails>> GetAllByInboundIdAsync(int inboundId)
        {
            return await _context.InboundDetails
                .Where(id => id.InboundId == inboundId)
                .ToListAsync();
        }

    }
}
