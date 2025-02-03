using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        protected readonly DrugWarehouseContext _context;
        public IAccountRepository AccountRepository { get; private set; }
        public IAuditLogsRepository AuditLogsRepository { get; private set; }
        public IOutboundDetailsRepository OutboundDetailsRepository { get; private set; }
        public IOutboundRepository OutboundRepository { get; private set; } 


		public UnitOfWork(DrugWarehouseContext context)
        {
            _context = context;
            AccountRepository ??= new AccountRepository(_context);
            AuditLogsRepository ??= new AuditLogsRepository(_context);
			OutboundRepository ??= new OutboundRepository(_context);
			OutboundDetailsRepository ??= new OutboundDetailRepostitory(_context);
			LotRepository ??= new LotRepository(_context);
		}

        public async Task BeginTransaction()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransaction()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransaction()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
