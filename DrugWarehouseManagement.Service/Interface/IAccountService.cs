using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IAccountService
    {
        public Task<AccountLoginResponse> LoginWithUsername(AccountLoginRequest request);
        public Task UpdateLastLogin(UpdateLastLoginDTO updateLastLoginDTO);
        public Task<SetupTwoFactorAuthenticatorResponse> SetupTwoFactorAuthenticator(string email);
        public Task<BaseResponse> CreateAccount(CreateAccountRequest request);
        public Task<BaseResponse> UpdateAccountSettings(Guid accountId, UpdateAccountSettingsRequest request);
        public Task<PaginatedResult<ViewAccount>> GetAccountsPaginatedAsync(QueryPaging request);
        public Task<ViewAccount> GetAccountById(Guid accountId);
        public Task<BaseResponse> DeleteAccount(Guid accountId);
        public Task<BaseResponse> UpdateAccount(Guid accountId, UpdateAccountRequest request);
        public Task<BaseResponse> ResetPassword(Guid accountId);
        public Task<BaseResponse> DeactiveAccount(Guid accountId);
        public Task<BaseResponse> ActiveAccount(Guid accountId);
    }
}
