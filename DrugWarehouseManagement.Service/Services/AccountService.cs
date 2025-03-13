using Azure.Core;
using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Wrapper.Interface;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace DrugWarehouseManagement.Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenHandlerService _tokenHandler;
        private readonly ITwoFactorAuthenticatorWrapper _twoFactorAuthenticator;
        private readonly ILogger<IAccountService> _logger;
        private readonly IEmailService _emailService;
        private readonly IPasswordWrapper _passwordHelper;

        public AccountService(
            IUnitOfWork unitOfWork,
            ITokenHandlerService tokenHandler,
            ILogger<IAccountService> logger,
            IEmailService emailService,
            ITwoFactorAuthenticatorWrapper twoFactorAuthenticatorWrapper, 
            IPasswordWrapper passwordHelper)
        {
            _unitOfWork = unitOfWork;
            _passwordHelper = passwordHelper;
            _tokenHandler = tokenHandler;
            _twoFactorAuthenticator ??= twoFactorAuthenticatorWrapper;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<BaseResponse> ActiveAccount(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.Status = AccountStatus.Active;
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = 200,
                Message = "Account activated successfully"
            };
        }

        public async Task<BaseResponse> AdminReset2FA(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.tOTPSecretKey = null;
            account.BackupCode = null;
            account.TwoFactorAuthenticatorStatus = TwoFactorAuthenticatorSetupStatus.NotStarted;
            account.OTPCode = null;
            account.TwoFactorEnabled = false;

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Two factor authenticator reset successfully"
            };
        }

        public async Task<BaseResponse> ChangePassword(Guid accountId, ChangePasswordRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var verifyPassword = _passwordHelper.VerifyHashedPassword(account, account.PasswordHash, request.OldPassword);

            if (verifyPassword == PasswordVerificationResult.Failed)
            {
                throw new Exception("Old password is incorrect");
            }

            var hashedPassword = _passwordHelper.HashPassword(account, request.NewPassword);
            account.PasswordHash = hashedPassword;
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Password changed successfully"
            };
        }

        public async Task<BaseResponse> ConfirmSetupTwoFactorAuthenticator(Guid accountId, ConfirmSetupTwoFactorAuthenticatorRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            if (account.TwoFactorAuthenticatorStatus == TwoFactorAuthenticatorSetupStatus.Completed)
            {
               throw new Exception("Two factor authenticator is already confirmed");
            }

            var verifyCode = VerifyTwoFactorCode(account.tOTPSecretKey, request.OTPCode.Trim());

            if (!verifyCode)
            {
                throw new Exception("Two factor code is incorrect");
            }

            if (account.OTPCode != null && request.OTPCode == Utils.Base64Decode(account.OTPCode))
            {
                throw new Exception("Two factor code is already used");
            }

            account.OTPCode = Utils.Base64Encode(request.OTPCode.Trim());
            account.TwoFactorEnabled = true;
            account.TwoFactorAuthenticatorStatus = TwoFactorAuthenticatorSetupStatus.Completed;

            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = 200,
                Message = "Two factor authenticator confirmed successfully"
            };
        }

        public async Task<BaseResponse> CreateAccount(CreateAccountRequest request)
        {
            var existedAccount = await _unitOfWork.AccountRepository
                        .GetByWhere(x => x.UserName == request.UserName.ToLower().Trim() ||
                                    x.Email == request.Email.ToLower().Trim() ||
                                    x.PhoneNumber == request.PhoneNumber.Trim())
                        .Include(x => x.Role)
                        .FirstOrDefaultAsync();

            if (existedAccount != null)
            {
                throw new Exception("Account already existed");
            }

            var account = request.Adapt<Account>();

            var randomPassword = Utils.GenerateRandomPassword();
            var hashedPassword = _passwordHelper.HashPassword(account, randomPassword);

            account.PasswordHash = hashedPassword;
            account.AccountSettings = new AccountSettings();

            await _unitOfWork.AccountRepository.CreateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            var htmlTemplate = Consts.htmlCreateAccountTemplate;

            htmlTemplate = htmlTemplate.Replace("{{Username}}", account.UserName)
                                       .Replace("{{Password}}", randomPassword);

            await _emailService.SendEmailAsync(account.Email, "Account Created", htmlTemplate);

            //_logger.LogWarning($"Account created with username: {account.Username} and password: {randomPassword}"); // For development purpose, should using email to send password to user

            return new BaseResponse
            {
                Code = 200,
                Message = "Account created successfully, please check your (spam) inbox for login credentials",
            };
        }

        public async Task<BaseResponse> DeactiveAccount(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.Status = AccountStatus.Inactive;
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = 200,
                Message = "Account deactivated successfully"
            };
        }

        public async Task<BaseResponse> DeleteAccount(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.Status = AccountStatus.Deleted;
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = 200,
                Message = "Account deleted successfully"
            };
        }

        public async Task<ViewAccount> GetAccountById(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByWhere(x => x.Id == accountId)
                        .Include(x => x.Role)
                        .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            return account.Adapt<ViewAccount>();
        }

        public async Task<PaginatedResult<ViewAccount>> GetAccountsPaginatedAsync(QueryPaging request)
        {
            request.Search = request.Search?.ToLower().Trim() ?? "";
            var query = await _unitOfWork.AccountRepository.GetAll()
                        .Include(x => x.Role)
                        .Where(x => x.Status == AccountStatus.Active)
                        .Where(x => x.UserName.Contains(request.Search) || x.Email.Contains(request.Search) || x.PhoneNumber.Contains(request.Search))
                        .ToPaginatedResultAsync(request.Page, request.PageSize);
            return query.Adapt<PaginatedResult<ViewAccount>>();
        }

        public async Task<AccountLoginResponse> LoginWithUsername(AccountLoginRequest request)
        {
            var account = await _unitOfWork.AccountRepository
                        .GetByWhere(x => x.UserName == request.Username.Trim())
                        .Include(x => x.Role)
                        .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            if (account.Status == AccountStatus.Inactive)
            {
                throw new Exception("Account is inactive, please contact your administrator to re-active your account");
            }

            if (account.TwoFactorEnabled)
            {
                if (request.OTPCode == null)
                {
                    throw new Exception("Two factor code is required");
                }

                var verify = VerifyTwoFactorCode(account.tOTPSecretKey, request.OTPCode.Trim());

                if (!verify)
                {
                    throw new Exception("Two factor code is incorrect");
                }

                if (!String.IsNullOrEmpty(account.OTPCode) && request.OTPCode == Utils.Base64Decode(account.OTPCode))
                {
                    throw new Exception("Two factor code is already used");
                }

                account.OTPCode = Utils.Base64Encode(request.OTPCode.Trim());
            }

            //if (account.TwoFactorEnabled) // Đang suy nghĩ luồng backup code
            //{
            //    if (request.OTPCode == null || request.BackupCode == null)
            //    {
            //        throw new Exception("Two factor code or backup code is required");
            //    }

            //    bool isTwoFactorCodeValid = false;
            //    PasswordVerificationResult isBackupCodeValid = PasswordVerificationResult.Failed;

            //    if (request.OTPCode != null)
            //    {
            //        isTwoFactorCodeValid = VerifyTwoFactorCode(account.tOTPSecretKey, request.OTPCode.Trim());

            //        if (!isTwoFactorCodeValid)
            //        {
            //            throw new Exception("Two factor code is incorrect");
            //        }

            //        if (!String.IsNullOrEmpty(account.OTPCode) && request.OTPCode == Utils.Base64Decode(account.OTPCode))
            //        {
            //            throw new Exception("Two factor code is already used");
            //        }

            //        account.OTPCode = Utils.Base64Encode(request.OTPCode.Trim());
            //    }

            //    if (request.BackupCode != null)
            //    {
            //        isBackupCodeValid = _passwordHelper.VerifyHashedValue(account.BackupCode, request.BackupCode);

            //        if (isBackupCodeValid == PasswordVerificationResult.Failed)
            //        {
            //            throw new Exception("Backup code is incorrect or already used");
            //        }
            //    }

            //    if (!isTwoFactorCodeValid)
            //    {
            //        throw new Exception("Invalid two factor code or backup code");
            //    }
            //}

            var verifyPassword = _passwordHelper.VerifyHashedPassword(account, account.PasswordHash, request.Password);

            if (verifyPassword == PasswordVerificationResult.Failed)
            {
                throw new Exception("Password is incorrect");
            }

            account.ConcurrencyStamp = Guid.NewGuid().ToString();

            await _unitOfWork.AccountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return new AccountLoginResponse
            {
                Role = account.Role.RoleName,
                RefreshToken = _tokenHandler.GenerateRefreshToken(account.Id),
                Token = _tokenHandler.GenerateJwtToken(account)
            };
        }

        public async Task<RefreshTokenResponse> GenerateRefreshToken(RefreshTokenRequest request)
        {
            var principal = _tokenHandler.ValidateRefreshToken(request.RefreshToken);
            if (principal == null)
            {
                throw new Exception("Invalid refresh token");
            }

            var accountId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var account = await _unitOfWork.AccountRepository.GetByWhere(acc => acc.Id == Guid.Parse(accountId))
                                                            .Include(x => x.Role)
                                                            .FirstOrDefaultAsync();
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            account.ConcurrencyStamp = Guid.NewGuid().ToString();
            await _unitOfWork.AccountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return new RefreshTokenResponse
            {
                Token = _tokenHandler.GenerateJwtToken(account),
            };
        }

        public async Task<BaseResponse> ResetPassword(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var randomPassword = Utils.GenerateRandomPassword();
            var hashedPassword = _passwordHelper.HashPassword(account, randomPassword);

            account.PasswordHash = hashedPassword;

            await _unitOfWork.SaveChangesAsync();

            var htmlTemplate = Consts.htmlResetPasswordTemplate;

            htmlTemplate = htmlTemplate.Replace("{{Username}}", account.UserName)
                                       .Replace("{{Password}}", randomPassword);

            await _emailService.SendEmailAsync(account.Email, "Reset Password", htmlTemplate);
            return new BaseResponse
            {
                Code = 200,
                Message = "Password reset successfully, please check your (spam) inbox for new login credentials",
            };
        }

        public async Task<SetupTwoFactorAuthenticatorResponse> SetupTwoFactorAuthenticator(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            if (account.TwoFactorEnabled)
            {
                throw new Exception("Two factor authenticator is already setup");
                //setupCode = _twoFactorAuthenticator.GenerateSetupCode("DrugWarehouse", email, account.tOTPSecretKey);
                //return new SetupTwoFactorAuthenticatorResponse
                //{
                //    ImageUrlQrCode = setupCode.QrCodeSetupImageUrl,
                //    ManualEntryKey = setupCode.ManualEntryKey
                //};
            }

            byte[] secretKey = new byte[16];
            RandomNumberGenerator.Fill(secretKey);

            var setupCode = _twoFactorAuthenticator.GenerateSetupCode("DrugWarehouse", account.Email, secretKey);
            var backupCode = Utils.Generate2FABackupCode(16);

            account.tOTPSecretKey = secretKey;
            account.BackupCode = _passwordHelper.HashValue(backupCode);
            account.TwoFactorAuthenticatorStatus = TwoFactorAuthenticatorSetupStatus.Pending;

            await _unitOfWork.AccountRepository.UpdateAsync(account);

            await _unitOfWork.SaveChangesAsync();

            return new SetupTwoFactorAuthenticatorResponse
            {
                ImageUrlQrCode = setupCode.QrCodeSetupImageUrl,
                ManualEntryKey = setupCode.ManualEntryKey,
                BackupCode = backupCode
            };
        }

        public async Task<BaseResponse> UpdateAccount(Guid accountId, UpdateAccountRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            request.Adapt(account);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Account updated successfully"
            };
        }

        public async Task<BaseResponse> UpdateAccountSettings(Guid accountId, UpdateAccountSettingsRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            if (account.AccountSettings == null)
            {
                account.AccountSettings = new AccountSettings();
            }

            if (request.PreferredLanguage != null && !Regex.Match(request.PreferredLanguage, @"^[a-zA-Z]{2}$").Success)
            {
                throw new Exception("Preferred language must be exactly 2 alphabetic characters");
            }

            request.Adapt(account.AccountSettings);
            await _unitOfWork.AccountRepository.UpdateAsync(account);

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Account settings updated successfully"
            };
        }

        private bool VerifyTwoFactorCode(byte[] secretKey, string code)
        {
            return _twoFactorAuthenticator.ValidateTwoFactorPIN(secretKey, code.Trim(), 0);
        }

    }
}
