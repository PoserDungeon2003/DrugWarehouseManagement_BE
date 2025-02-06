using DrugWarehouseManagement.Repository.Interface;

namespace DrugWarehouseManagement.Repository
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
        IAccountRepository AccountRepository { get; }
        IOutboundRepository OutboundRepository { get; }
        IOutboundDetailsRepository OutboundDetailsRepository { get; }
        IProductRepository ProductRepository { get; }
        ILotRepository LotRepository { get; }
        IAuditLogsRepository AuditLogsRepository { get; }
        IProviderRepository ProviderRepository { get; }
    }
}
