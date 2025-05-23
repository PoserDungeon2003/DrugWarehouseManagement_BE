﻿using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IAccountService
    {
        public Task<AccountLoginResponse> LoginWithUsername(AccountLoginRequest request);
        public Task<SetupTwoFactorAuthenticatorResponse> SetupTwoFactorAuthenticator(Guid accountId);
        public Task<BaseResponse> CreateAccount(CreateAccountRequest request);
        public Task<BaseResponse> UpdateAccountSettings(Guid accountId, UpdateAccountSettingsRequest request);
        public Task<PaginatedResult<ViewAccount>> GetAccountsPaginatedAsync(QueryPaging request);
        public Task<ViewAccount> GetAccountById(Guid accountId);
        public Task<BaseResponse> DeleteAccount(Guid accountId);
        public Task<BaseResponse> UpdateAccount(Guid accountId, UpdateAccountRequest request);
        public Task<BaseResponse> ResetPassword(string email);
        public Task<BaseResponse> DeactiveAccount(Guid accountId);
        public Task<BaseResponse> ActiveAccount(Guid accountId);
        public Task<BaseResponse> ChangePassword(Guid accountId, ChangePasswordRequest request);
        public Task<BaseResponse> ConfirmSetupTwoFactorAuthenticator(Guid accountId, ConfirmSetupTwoFactorAuthenticatorRequest request);
        public Task<BaseResponse> AdminReset2FA(Guid accountId);
        public Task<RefreshTokenResponse> GenerateRefreshToken(RefreshTokenRequest request);
    }
}
