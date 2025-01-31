using DrugWarehouseManagement.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
        IAccountRepository AccountRepository { get; }
        IAuditLogsRepository AuditLogsRepository { get; }
    }
}
