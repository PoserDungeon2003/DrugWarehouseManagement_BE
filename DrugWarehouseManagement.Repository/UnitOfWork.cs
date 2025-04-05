using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository.Repositories;

namespace DrugWarehouseManagement.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        protected readonly DrugWarehouseContext _context;
        public ILotRepository LotRepository { get; private set; }
        public IAccountRepository AccountRepository { get; private set; }
        public IAuditLogsRepository AuditLogsRepository { get; private set; }
        public IInboundRepository InboundRepository { get; private set; }
        public IInboundDetailRepository InboundDetailRepository { get; private set; }
        public IInboundRequestRepository InboundRequestRepository { get; private set; }
        public IInboundRequestDetailsRepository InboundRequestDetailsRepository { get; private set; }
        public IInboundRequestAssetsRepository InboundRequestAssetsRepository { get; private set; }
        public IInboundReportRepository InboundReportRepository { get; private set; }
        public IInboundReportAssetsRepository InboundReportAssetsRepository { get; private set; }
        public IOutboundDetailsRepository OutboundDetailsRepository { get; private set; }
        public IOutboundRepository OutboundRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public IProviderRepository ProviderRepository { get; private set; }
        public IWarehouseRepository WarehouseRepository { get; private set; }
        public ICustomerRepository CustomerRepository { get; private set; }
        public ILotTransferRepository LotTransferRepository { get; private set; }
        public ILotTransferDetailRepository LotTransferDetailsRepository { get; private set; }
        public ICategoriesRepository CategoriesRepository { get; private set; }
        public IReturnOutboundDetailsRepository ReturnOutboundDetailsRepository { get; private set; }
        public IDeviceRepository DeviceRepository { get; private set; }
        public IProductCategoriesRepository ProductCategoriesRepository { get; private set; }

        public UnitOfWork(DrugWarehouseContext context)
        {
            _context = context;
            AccountRepository ??= new AccountRepository(_context);
            AuditLogsRepository ??= new AuditLogsRepository(_context);
            InboundRepository ??= new InboundRepository(_context);
            InboundDetailRepository ??= new InboundDetailRepository(_context);
            InboundRequestRepository ??= new InboundRequestRepository(_context);
            InboundRequestDetailsRepository ??= new InboundRequestDetailsRepository(_context);
            InboundRequestAssetsRepository ??= new InboundRequestAssetsRepository(_context);
            InboundReportRepository ??= new InboundReportRepository(_context);
            InboundReportAssetsRepository ??= new InboundReportAssetsRepository(_context);
            OutboundRepository ??= new OutboundRepository(_context);
            OutboundDetailsRepository ??= new OutboundDetailRepostitory(_context);
            LotRepository ??= new LotRepository(_context);
            ProductRepository ??= new ProductRepository(_context);
            ProviderRepository ??= new ProviderRepository(_context);
            WarehouseRepository ??= new WarehouseRepository(_context);
            CustomerRepository ??= new CustomerRepository(_context);
            LotTransferRepository ??= new LotTransferRepository(_context);
            LotTransferDetailsRepository ??= new LotTransferDetailRepository(_context);
            CategoriesRepository ??= new CategoriesRepository(_context);
            ReturnOutboundDetailsRepository ??= new ReturnOutboundDetailsRepository(_context);
            DeviceRepository ??= new DeviceRepository(_context);
            ProductCategoriesRepository ??= new ProductCategoriesRepository(_context);
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
