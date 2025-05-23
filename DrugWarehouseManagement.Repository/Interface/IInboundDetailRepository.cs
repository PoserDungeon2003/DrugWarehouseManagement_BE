﻿using DrugWarehouseManagement.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Interface
{
    public interface IInboundDetailRepository : IGenericRepository<InboundDetails>
    {
        Task<IEnumerable<InboundDetails>> GetAllByInboundIdAsync(int inboundId);
    }
}
