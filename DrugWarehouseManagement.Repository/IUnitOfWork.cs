using DrugWarehouseManagement.Repository.Interface;

namespace DrugWarehouseManagement.Repository
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
        ICustomerRepository CustomerRepository { get; }
        IAccountRepository AccountRepository { get; }
        IWarehouseRepository WarehouseRepository { get; }
        IInboundRepository InboundRepository { get; }
        IInboundDetailRepository InboundDetailRepository { get; }
        IOutboundRepository OutboundRepository { get; }
        IOutboundDetailsRepository OutboundDetailsRepository { get; }
        IProductRepository ProductRepository { get; }
        ILotRepository LotRepository { get; }
        IAuditLogsRepository AuditLogsRepository { get; }
        IProviderRepository ProviderRepository { get; }
        ILotTransferRepository LotTransferRepository { get; }
        ILotTransferDetailRepository LotTransferDetailsRepository { get; }
        ICategoriesRepository CategoriesRepository { get; }
    }
}
