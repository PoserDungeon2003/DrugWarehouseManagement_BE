using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface ITokenHandlerService
    {
        public string GenerateJwtToken(Account account);
    }
}
