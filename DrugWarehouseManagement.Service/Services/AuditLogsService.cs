using Azure.Core;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class AuditLogsService : IAuditLogsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuditLogsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PaginatedResult<ViewAuditLogs>> ViewLogsAsync(QueryPaging queryPaging)
        {
            queryPaging.Search = queryPaging.Search?.ToLower().Trim() ?? "";
            var logs = await _unitOfWork.AuditLogsRepository.GetAll()
                                            .Include(a => a.Account)
                                            .Where(al => al.Action.Contains(queryPaging.Search))
                                            .OrderByDescending(al => al.Date)
                                            .ToPaginatedResultAsync(queryPaging.Page, queryPaging.PageSize);
            return logs.Adapt<PaginatedResult<ViewAuditLogs>>();
        }
    }
}
