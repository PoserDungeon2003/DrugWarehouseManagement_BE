using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Common.Consts;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Request;
using Google.Authenticator;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly ITokenHandlerService _tokenHandler;
        private readonly TwoFactorAuthenticator _twoFactorAuthenticator;
        private readonly ILogger<IAccountService> _logger;
        private readonly IEmailService _emailService;

        public AccountService(IUnitOfWork unitOfWork, ITokenHandlerService tokenHandler, ILogger<IAccountService> logger, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher ??= new PasswordHasher<string>();
            _tokenHandler = tokenHandler;
            _twoFactorAuthenticator ??= new TwoFactorAuthenticator();
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<BaseResponse> CreateAccount(CreateAccountRequest request)
        {
            var existedAccount = await _unitOfWork.AccountRepository
                        .GetByWhere(x => x.Username == request.Username.ToLower().Trim() || 
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
            var hashedPassword = HashPassword(randomPassword);

            account.Password = hashedPassword;
            account.AccountSettings = new AccountSettings();

            await _unitOfWork.AccountRepository.CreateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            var htmlTemplate = Consts.htmlCreateAccountTemplate;

            htmlTemplate = htmlTemplate.Replace("{{Username}}", account.Username)
                                       .Replace("{{Password}}", randomPassword);

            await _emailService.SendEmailAsync(account.Email, "Account Created", htmlTemplate);

            //_logger.LogWarning($"Account created with username: {account.Username} and password: {randomPassword}"); // For development purpose, should using email to send password to user

            return new BaseResponse
            { 
                Code = 200,
                Message = "Account created successfully, please check your (spam) inbox for login credentials",
            };
        }

        public async Task<ViewAccount> GetAccountById(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByWhere(x => x.AccountId == accountId)
                        .Include(x => x.Role)
                        .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            return account.Adapt<ViewAccount>();
        }

        public async Task<PaginatedResult<ViewAccount>> GetAccountsPaginatedAsync(int page = 1, int pageSize = 10)
        {
            var query = await _unitOfWork.AccountRepository.GetAll()
                        .Include(x => x.Role)   
                        .OrderByDescending(x => x.CreatedAt)
                        .ToPaginatedResultAsync(page, pageSize);
            return query.Adapt<PaginatedResult<ViewAccount>>();
        }

        public async Task<AccountLoginResponse> LoginWithUsername(AccountLoginRequest request)
        {
            var account = await _unitOfWork.AccountRepository
                        .GetByWhere(x => x.Username == request.Username.Trim())
                        .Include(x => x.Role)
                        .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            if (account.Status == Common.Enums.AccountStatus.Inactive)
            {
                throw new Exception("Account is inactive, please contact your administrator to re-active your account");
            }

            if (account.AccountSettings != null && account.AccountSettings.IsTwoFactorEnabled)
            {
                if (request.tOtpCode == null)
                {
                    throw new Exception("Two factor code is required");
                }

                var verify = _twoFactorAuthenticator.ValidateTwoFactorPIN(account.tOTPSecretKey, request.tOtpCode.Trim(), 0);

                if (!verify)
                {
                    throw new Exception("Two factor code is incorrect");
                }

                if (!String.IsNullOrEmpty(account.OTPCode) && request.tOtpCode == Utils.Base64Decode(account.OTPCode))
                {
                    throw new Exception("This code has been used");
                }

                account.OTPCode = Utils.Base64Encode(request.tOtpCode.Trim());
            }

            var verifyPassword = _passwordHasher.VerifyHashedPassword(null, account.Password, request.Password);

            if (verifyPassword == PasswordVerificationResult.Failed)
            {
                throw new Exception("Password is incorrect");
            }

            await _unitOfWork.AccountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            await UpdateLastLogin(new UpdateLastLoginDTO
            {
                AccountId = account.AccountId,
                LastLogin = SystemClock.Instance.GetCurrentInstant()
            });

            return new AccountLoginResponse
            {
                Role = account.Role.RoleName,
                Token = _tokenHandler.GenerateJwtToken(account)
            };
        }

        public async Task<SetupTwoFactorAuthenticatorResponse> SetupTwoFactorAuthenticator(string email)
        {
            SetupCode setupCode;

            var account = await _unitOfWork.AccountRepository.GetByWhere(x => x.Email == email.Trim()).FirstOrDefaultAsync();

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            if (account.AccountSettings != null && account.AccountSettings.IsTwoFactorEnabled)
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

            setupCode = _twoFactorAuthenticator.GenerateSetupCode("DrugWarehouse", email, secretKey);

            account.tOTPSecretKey = secretKey;

            if (account.AccountSettings == null)
            {
                account.AccountSettings = new AccountSettings();
            }

            await _unitOfWork.AccountRepository.UpdateAsync(account);

            await _unitOfWork.SaveChangesAsync();

            return new SetupTwoFactorAuthenticatorResponse
            {
                ImageUrlQrCode = setupCode.QrCodeSetupImageUrl,
                ManualEntryKey = setupCode.ManualEntryKey
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

            if (request.PreferredLanguage != null && !Regex.Match(request.PreferredLanguage, @"^[a-zA-Z]{2}$").Success) {
                throw new Exception("Preferred language must be exactly 2 alphabetic characters");
            }

            request.Adapt(account.AccountSettings);
            await _unitOfWork.AccountRepository.UpdateAsync(account);

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse {
                Code = 200,
                Message = "Account settings updated successfully"
            };
        }

        public async Task UpdateLastLogin(UpdateLastLoginDTO updateLastLoginDTO)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(updateLastLoginDTO.AccountId);

            if (account == null)
            {
               throw new Exception("Account not found");
            }

            var updatedAccount = updateLastLoginDTO.Adapt(account);

            await _unitOfWork.AccountRepository.UpdateAsync(updatedAccount);

            await _unitOfWork.SaveChangesAsync();
        }

        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }
    }
}
