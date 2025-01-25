using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Request;
using Google.Authenticator;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly TokenHandlerService _tokenHandler;
        private readonly TwoFactorAuthenticator _twoFactorAuthenticator;

        public AccountService(IUnitOfWork unitOfWork, TokenHandlerService tokenHandler)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher ??= new PasswordHasher<string>();
            _tokenHandler = tokenHandler;
            _twoFactorAuthenticator ??= new TwoFactorAuthenticator();
        }

        public async Task<AccountLoginResponse> LoginWithEmail(AccountLoginRequest request)
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
                throw new Exception("Account is inactive, please re-active your account");
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

            var account = await _unitOfWork.AccountRepository.GetByWhere(x => x.Email == email).FirstOrDefaultAsync();

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

            account.AccountSettings.IsTwoFactorEnabled = true;

            await _unitOfWork.AccountRepository.UpdateAsync(account);

            await _unitOfWork.SaveChangesAsync();

            return new SetupTwoFactorAuthenticatorResponse
            {
                ImageUrlQrCode = setupCode.QrCodeSetupImageUrl,
                ManualEntryKey = setupCode.ManualEntryKey
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
    }
}
