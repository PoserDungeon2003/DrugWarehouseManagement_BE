using DrugWarehouseManagement.Repository.Models;
using System.Security.Claims;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface ITokenHandlerService
    {
        public string GenerateJwtToken(Account account);
        public string GenerateRefreshToken(Guid accountId);
        public ClaimsPrincipal? ValidateRefreshToken(string token);
    }
}
