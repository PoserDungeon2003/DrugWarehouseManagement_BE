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
        IInboundRequestRepository InboundRequestRepository { get; }
        IInboundRequestDetailsRepository InboundRequestDetailsRepository { get; }
        IInboundReportRepository InboundReportRepository { get; }
        IInboundReportAssetsRepository InboundReportAssetsRepository { get; }
        IOutboundRepository OutboundRepository { get; }
        IOutboundDetailsRepository OutboundDetailsRepository { get; }
        IProductRepository ProductRepository { get; }
        ILotRepository LotRepository { get; }
        IAuditLogsRepository AuditLogsRepository { get; }
        IProviderRepository ProviderRepository { get; }
        ILotTransferRepository LotTransferRepository { get; }
        ILotTransferDetailRepository LotTransferDetailsRepository { get; }
        ICategoriesRepository CategoriesRepository { get; }
        IReturnOutboundDetailsRepository ReturnOutboundDetailsRepository { get; }
        IDeviceRepository DeviceRepository { get; }
        IProductCategoriesRepository ProductCategoriesRepository { get; }
        IAssetRepository AssetRepository { get; }
    }
}
