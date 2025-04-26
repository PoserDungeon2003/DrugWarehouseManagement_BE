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
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public AccountService(
            IUnitOfWork unitOfWork,
            ITokenHandlerService tokenHandler,
            ILogger<IAccountService> logger,
            IEmailService emailService,
            ITwoFactorAuthenticatorWrapper twoFactorAuthenticatorWrapper,
            IPasswordWrapper passwordHelper,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _passwordHelper = passwordHelper;
            _tokenHandler = tokenHandler;
            _twoFactorAuthenticator ??= twoFactorAuthenticatorWrapper;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<BaseResponse> ActiveAccount(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Không tìm thấy tài khoản");
            }

            account.Status = AccountStatus.Active;
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = 200,
                Message = "Tài khoản đã được kích hoạt thành công"
            };
        }

        public async Task<BaseResponse> AdminReset2FA(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Không tìm thấy tài khoản");
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
                Message = "Tài khoản đã được reset 2FA thành công"
            };
        }

        public async Task<BaseResponse> ChangePassword(Guid accountId, ChangePasswordRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Không tìm thấy tài khoản");
            }

            var verifyPassword = _passwordHelper.VerifyHashedPassword(account, account.PasswordHash, request.OldPassword);

            if (verifyPassword == PasswordVerificationResult.Failed)
            {
                throw new Exception("Mật khẩu cũ không chính xác");
            }

            var hashedPassword = _passwordHelper.HashPassword(account, request.NewPassword);
            account.PasswordHash = hashedPassword;
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Đổi mật khẩu thành công",
            };
        }

        public async Task<BaseResponse> ConfirmSetupTwoFactorAuthenticator(Guid accountId, ConfirmSetupTwoFactorAuthenticatorRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Không tìm thấy tài khoản");
            }

            if (account.TwoFactorAuthenticatorStatus == TwoFactorAuthenticatorSetupStatus.Completed)
            {
                throw new Exception("Xác thực 2FA đã hoàn tất");
            }

            var verifyCode = VerifyTwoFactorCode(account.tOTPSecretKey, request.OTPCode.Trim());

            if (!verifyCode)
            {
                throw new Exception("Mã xác thực 2FA không chính xác");
            }

            if (account.OTPCode != null && request.OTPCode == Utils.Base64Decode(account.OTPCode))
            {
                throw new Exception("Mã xác thực 2FA đã được sử dụng trước đó");
            }

            account.OTPCode = Utils.Base64Encode(request.OTPCode.Trim());
            account.TwoFactorEnabled = true;
            account.TwoFactorAuthenticatorStatus = TwoFactorAuthenticatorSetupStatus.Completed;
            var backupCode = Utils.Generate2FABackupCode(16);
            account.BackupCode = _passwordHelper.HashValue(backupCode);

            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = 200,
                Message = "Xác thực 2FA thành công",
                Result = new
                {
                    BackupCode = backupCode,
                }
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
                throw new Exception("Tài khoản đã tồn tại");
            }

            var account = request.Adapt<Account>();

            var randomPassword = Utils.GenerateRandomPassword();
            var hashedPassword = _passwordHelper.HashPassword(account, randomPassword);

            account.PasswordHash = hashedPassword;
            account.AccountSettings = new AccountSettings();


            var htmlTemplate = Consts.htmlCreateAccountTemplate;
            var loginPage = $"{_configuration.GetValue<string>("WebsiteUrl")}/login";

            htmlTemplate = htmlTemplate.Replace("{{Username}}", account.UserName)
                                       .Replace("{{Password}}", randomPassword)
                                       .Replace("{{WEBSITE_URL}}", loginPage);

            await _emailService.SendEmailAsync(account.Email, "Tài khoản đã được tạo", htmlTemplate);

            await _unitOfWork.AccountRepository.CreateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            //_logger.LogWarning($"Account created with username: {account.Username} and password: {randomPassword}"); // For development purpose, should using email to send password to user

            return new BaseResponse
            {
                Code = 200,
                Message = "Tài khoản đã được tạo thành công, vui lòng kiểm tra email để biết thông tin đăng nhập",
            };
        }

        public async Task<BaseResponse> DeactiveAccount(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại");
            }

            account.Status = AccountStatus.Inactive;
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = 200,
                Message = "Tài khoản đã được vô hiệu hóa thành công"
            };
        }

        public async Task<BaseResponse> DeleteAccount(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại");
            }

            account.Status = AccountStatus.Deleted;
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = 200,
                Message = "Tài khoản đã được xóa thành công"
            };
        }

        public async Task<ViewAccount> GetAccountById(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByWhere(x => x.Id == accountId)
                        .Include(x => x.Role)
                        .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại");
            }

            return account.Adapt<ViewAccount>();
        }

        public async Task<PaginatedResult<ViewAccount>> GetAccountsPaginatedAsync(QueryPaging request)
        {
            request.Search = request.Search?.ToLower().Trim() ?? "";
            var query = await _unitOfWork.AccountRepository.GetAll()
                        .Include(x => x.Role)
                        .Where(x => x.Status == AccountStatus.Active)
                        .Where(x => x.UserName.ToLower().Contains(request.Search) || x.Email.ToLower().Contains(request.Search) || x.PhoneNumber.ToLower().Contains(request.Search))
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
                throw new Exception("Tài khoản không tồn tại");
            }

            if (account.Status == AccountStatus.Inactive)
            {
                throw new Exception("Tài khoản đã bị vô hiệu hóa, vui lòng liên hệ với quản trị viên để kích hoạt lại tài khoản");
            }

            if (account.Status == AccountStatus.Deleted)
            {
                throw new Exception("Tài khoản đã bị xóa, vui lòng liên hệ với quản trị viên để biết thêm thông tin");
            }

            var verifyPassword = _passwordHelper.VerifyHashedPassword(account, account.PasswordHash, request.Password);

            if (verifyPassword == PasswordVerificationResult.Failed)
            {
                throw new Exception("Mật khẩu không chính xác");
            }

            if (account.TwoFactorEnabled)
            {
                if (request.OTPCode == null && (request.LostOTPCode != true || request.BackupCode == null))
                {
                    return new AccountLoginResponse
                    {
                        RequiresTwoFactor = true,
                    };
                }
                if (request.LostOTPCode == true)
                {
                    if (request.BackupCode == null)
                    {
                        throw new Exception("Mã dự phòng là bắt buộc");
                    }
                    var verify = _passwordHelper.VerifyHashedValue(account.BackupCode, request.BackupCode.Trim());

                    if (verify == PasswordVerificationResult.Failed)
                    {
                        throw new Exception("Mã dự phòng không chính xác");
                    }
                }
                else
                {
                    if (request.OTPCode == null)
                    {
                        throw new Exception("Mã xác thực 2FA là bắt buộc");
                    }

                    var verify = VerifyTwoFactorCode(account.tOTPSecretKey, request.OTPCode.Trim());

                    if (!verify)
                    {
                        throw new Exception("Mã xác thực 2FA không chính xác");
                    }

                    if (!String.IsNullOrEmpty(account.OTPCode) && request.OTPCode == Utils.Base64Decode(account.OTPCode))
                    {
                        throw new Exception("Mã xác thực 2FA đã được sử dụng trước đó");
                    }

                    account.OTPCode = Utils.Base64Encode(request.OTPCode.Trim());

                }
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
                throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn");
            }

            var accountId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            var account = await _unitOfWork.AccountRepository.GetByWhere(acc => acc.Id == Guid.Parse(accountId))
                                                            .Include(x => x.Role)
                                                            .FirstOrDefaultAsync();
            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại");
            }

            account.ConcurrencyStamp = Guid.NewGuid().ToString();
            await _unitOfWork.AccountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return new RefreshTokenResponse
            {
                Token = _tokenHandler.GenerateJwtToken(account),
            };
        }

        public async Task<BaseResponse> ResetPassword(string email)
        {
            var account = await _unitOfWork.AccountRepository.GetByWhere(acc => acc.Email == email.ToLower().Trim())
                        .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại");
            }

            var randomPassword = Utils.GenerateRandomPassword();
            var hashedPassword = _passwordHelper.HashPassword(account, randomPassword);

            account.PasswordHash = hashedPassword;

            var htmlTemplate = Consts.htmlResetPasswordTemplate;
            var loginPage = $"{_configuration.GetValue<string>("WebsiteUrl")}/login";

            htmlTemplate = htmlTemplate.Replace("{{Username}}", account.UserName)
                                       .Replace("{{Password}}", randomPassword)
                                       .Replace("{{WEBSITE_URL}}", loginPage);

            await _emailService.SendEmailAsync(account.Email, "Đặt lại mật khẩu", htmlTemplate);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Mật khẩu đã được đặt lại thành công, vui lòng kiểm tra email để biết thông tin đăng nhập",
            };
        }

        public async Task<SetupTwoFactorAuthenticatorResponse> SetupTwoFactorAuthenticator(Guid accountId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại");
            }

            if (account.TwoFactorEnabled)
            {
                throw new Exception("Tài khoản đã được kích hoạt xác thực 2FA trước đó");
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

            account.tOTPSecretKey = secretKey;
            // account.BackupCode = _passwordHelper.HashValue(backupCode);
            account.TwoFactorAuthenticatorStatus = TwoFactorAuthenticatorSetupStatus.Pending;

            await _unitOfWork.AccountRepository.UpdateAsync(account);

            await _unitOfWork.SaveChangesAsync();

            return new SetupTwoFactorAuthenticatorResponse
            {
                ImageUrlQrCode = setupCode.QrCodeSetupImageUrl,
                ManualEntryKey = setupCode.ManualEntryKey,
            };
        }

        public async Task<BaseResponse> UpdateAccount(Guid accountId, UpdateAccountRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại");
            }

            request.Adapt(account);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Cập nhật tài khoản thành công",
            };
        }

        public async Task<BaseResponse> UpdateAccountSettings(Guid accountId, UpdateAccountSettingsRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);

            if (account == null)
            {
                throw new Exception("Tài khoản không tồn tại");
            }

            if (account.AccountSettings == null)
            {
                account.AccountSettings = new AccountSettings();
            }

            if (request.PreferredLanguage != null && !Regex.Match(request.PreferredLanguage, @"^[a-zA-Z]{2}$").Success)
            {
                throw new Exception("Ngôn ngữ không hợp lệ, vui lòng nhập mã ngôn ngữ 2 ký tự (ví dụ: 'en', 'vi')");
            }

            request.Adapt(account.AccountSettings);
            await _unitOfWork.AccountRepository.UpdateAsync(account);

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Cập nhật cài đặt tài khoản thành công",
            };
        }

        private bool VerifyTwoFactorCode(byte[] secretKey, string code)
        {
            return _twoFactorAuthenticator.ValidateTwoFactorPIN(secretKey, code.Trim(), 0);
        }

    }
}
