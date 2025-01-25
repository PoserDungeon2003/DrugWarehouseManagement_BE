using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
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
        public Task<AccountSettings> UpdateAccountSettings(Guid accountId, UpdateAccountSettingsRequest request);
    }
}
