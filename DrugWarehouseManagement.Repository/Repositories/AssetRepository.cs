using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Repository.Repositories;

public class AssetRepository : GenericRepository<Asset>, IAssetRepository
{
  public AssetRepository(DrugWarehouseContext context) : base(context)
  {
  }
}