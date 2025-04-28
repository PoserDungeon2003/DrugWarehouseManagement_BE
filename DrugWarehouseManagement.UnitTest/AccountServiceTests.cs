using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using DrugWarehouseManagement.Service.Wrapper.Interface;
using Google.Authenticator;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MockQueryable;
using Moq;
using System.Linq.Expressions;

namespace DrugWarehouseManagement.UnitTest
{
    public class AccountServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ITokenHandlerService> _tokenHandlerMock;
        private readonly Mock<ITwoFactorAuthenticatorWrapper> _twoFactorAuthenticatorMock;
        private readonly Mock<ILogger<IAccountService>> _loggerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IPasswordWrapper> _passwordHelperMock;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;

        public AccountServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _passwordHelperMock = new Mock<IPasswordWrapper>();
            _tokenHandlerMock = new Mock<ITokenHandlerService>();
            _twoFactorAuthenticatorMock = new Mock<ITwoFactorAuthenticatorWrapper>();
            _loggerMock = new Mock<ILogger<IAccountService>>();
            _emailServiceMock = new Mock<IEmailService>();
            _passwordHasher ??= new PasswordHasher<Account>();
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"WebsiteUrl", "https://trung-hanh-management-fe.vercel.app/login"},
                })
                .Build();

            _accountService = new AccountService(
                _unitOfWorkMock.Object,
                _tokenHandlerMock.Object,
                _loggerMock.Object,
                _emailServiceMock.Object,
                _twoFactorAuthenticatorMock.Object,
                _passwordHelperMock.Object,
                _configuration
            );
        }

        [Fact]
        public async Task LoginWithUsername_AccountNotFound_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "nonexistent", Password = "password" };
            var mockAccounts = new List<Account>().AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _accountService.LoginWithUsername(request)
            );
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_AccountInactive_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "inactive", Password = "password" };
            var account = new Account
            {
                UserName = "inactive",
                Status = AccountStatus.Inactive,
                Role = new Role { RoleName = "User" }
            };

            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _accountService.LoginWithUsername(request)
            );
            Assert.Equal("Tài khoản đã bị vô hiệu hóa, vui lòng liên hệ với quản trị viên để kích hoạt lại tài khoản", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_AccountDeleted_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "deleted", Password = "password" };
            var account = new Account
            {
                UserName = "deleted",
                Status = AccountStatus.Deleted,
                Role = new Role { RoleName = "User" }
            };

            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _accountService.LoginWithUsername(request)
            );
            Assert.Equal("Tài khoản đã bị xóa, vui lòng liên hệ với quản trị viên để biết thêm thông tin", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_IncorrectPassword_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "testuser", Password = "wrongpassword" };
            var account = new Account
            {
                UserName = "testuser",
                Status = AccountStatus.Active,
                Role = new Role { RoleName = "User" }
            };

            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Failed);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _accountService.LoginWithUsername(request)
            );
            Assert.Equal("Mật khẩu không chính xác", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_TwoFactorEnabledNoCode_RequiresTwoFactor()
        {
            // Arrange
            var request = new AccountLoginRequest
            {
                Username = "2fauser",
                Password = "password"
            };

            var account = new Account
            {
                UserName = "2fauser",
                Status = AccountStatus.Active,
                TwoFactorEnabled = true,
                Role = new Role { RoleName = "User" }
            };

            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            // Act
            var result = await _accountService.LoginWithUsername(request);

            // Assert
            Assert.True(result.RequiresTwoFactor);
            Assert.Null(result.Token);
            Assert.Null(result.RefreshToken);
        }

        [Fact]
        public async Task LoginWithUsername_TwoFactorEnabledWithBackupCode_SuccessfulLogin()
        {
            // Arrange
            var request = new AccountLoginRequest
            {
                Username = "2fauser",
                Password = "password",
                LostOTPCode = true,
                BackupCode = "backup123"
            };

            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserName = "2fauser",
                Status = AccountStatus.Active,
                TwoFactorEnabled = true,
                BackupCode = "hashedBackupCode",
                Role = new Role { RoleName = "User" }
            };

            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            _passwordHelperMock.Setup(p => p.VerifyHashedValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _tokenHandlerMock.Setup(t => t.GenerateRefreshToken(account.Id))
                .Returns("refreshToken123");

            _tokenHandlerMock.Setup(t => t.GenerateJwtToken(account))
                .Returns("jwtToken123");

            // Act
            var result = await _accountService.LoginWithUsername(request);

            // Assert
            Assert.Equal("User", result.Role);
            Assert.Equal("refreshToken123", result.RefreshToken);
            Assert.Equal("jwtToken123", result.Token);
            Assert.False(result.RequiresTwoFactor);
            _unitOfWorkMock.Verify(u => u.AccountRepository.UpdateAsync(It.IsAny<Account>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LoginWithUsername_TwoFactorEnabledWithOTPCode_SuccessfulLogin()
        {
            // Arrange
            var request = new AccountLoginRequest
            {
                Username = "2fauser",
                Password = "password",
                OTPCode = "123456"
            };

            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserName = "2fauser",
                Status = AccountStatus.Active,
                TwoFactorEnabled = true,
                tOTPSecretKey = new byte[] { 1, 2, 3, 4, 5 },
                Role = new Role { RoleName = "Admin" }
            };

            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            _twoFactorAuthenticatorMock.Setup(t => t.ValidateTwoFactorPIN(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _tokenHandlerMock.Setup(t => t.GenerateRefreshToken(account.Id))
                .Returns("refreshToken123");

            _tokenHandlerMock.Setup(t => t.GenerateJwtToken(account))
                .Returns("jwtToken123");

            // Act
            var result = await _accountService.LoginWithUsername(request);

            // Assert
            Assert.Equal("Admin", result.Role);
            Assert.Equal("refreshToken123", result.RefreshToken);
            Assert.Equal("jwtToken123", result.Token);
            Assert.False(result.RequiresTwoFactor);
            _unitOfWorkMock.Verify(u => u.AccountRepository.UpdateAsync(It.IsAny<Account>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task LoginWithUsername_TwoFactorEnabledWithInvalidOTPCode_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest
            {
                Username = "2fauser",
                Password = "password",
                OTPCode = "123456"
            };

            var account = new Account
            {
                UserName = "2fauser",
                Status = AccountStatus.Active,
                TwoFactorEnabled = true,
                tOTPSecretKey = new byte[] { 1, 2, 3, 4, 5 },
                Role = new Role { RoleName = "User" }
            };

            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            _twoFactorAuthenticatorMock.Setup(t => t.ValidateTwoFactorPIN(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _accountService.LoginWithUsername(request)
            );
            Assert.Equal("Mã xác thực 2FA không chính xác", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_TwoFactorEnabledWithPreviouslyUsedOTPCode_ThrowsException()
        {
            // Arrange
            var otpCode = "123456";
            var request = new AccountLoginRequest
            {
                Username = "2fauser",
                Password = "password",
                OTPCode = otpCode
            };

            var account = new Account
            {
                UserName = "2fauser",
                Status = AccountStatus.Active,
                TwoFactorEnabled = true,
                tOTPSecretKey = new byte[] { 1, 2, 3, 4, 5 },
                OTPCode = Utils.Base64Encode(otpCode),
                Role = new Role { RoleName = "User" }
            };

            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);

            _twoFactorAuthenticatorMock.Setup(t => t.ValidateTwoFactorPIN(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _accountService.LoginWithUsername(request)
            );
            Assert.Equal("Mã xác thực 2FA đã được sử dụng trước đó", exception.Message);
        }

        [Fact]
        public async Task CreateAccount_AccountAlreadyExists_ThrowsException()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                PhoneNumber = "1234567890"
            };
            var existingAccount = new Account { UserName = "testuser" };
            var mockAccounts = new List<Account> { existingAccount }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.CreateAccount(request));
            Assert.Equal("Tài khoản đã tồn tại", exception.Message);
        }

        [Fact]
        public async Task CreateAccount_SuccessfulCreation_ReturnsBaseResponse()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                UserName = "newuser",
                Email = "newuser@example.com",
                PhoneNumber = "0987654321"
            };
            var mockAccounts = new List<Account>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            _unitOfWorkMock.Setup(u => u.AccountRepository.CreateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _accountService.CreateAccount(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(200, response.Code);
            Assert.Equal("Tài khoản đã được tạo thành công, vui lòng kiểm tra email để biết thông tin đăng nhập", response.Message);
            _unitOfWorkMock.Verify(u => u.AccountRepository.CreateAsync(It.IsAny<Account>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _emailServiceMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SetupTwoFactorAuthenticator_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var mockAccounts = new List<Account>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.SetupTwoFactorAuthenticator(accountId));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task SetupTwoFactorAuthenticator_AlreadySetup_ThrowsException()
        {
            // Arrange
            var email = "testuser@example.com";
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Email = email,
                TwoFactorEnabled = true,
            };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(account.Id))
                .ReturnsAsync(account);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.SetupTwoFactorAuthenticator(account.Id));
            Assert.Equal("Tài khoản đã được kích hoạt xác thực 2FA trước đó", exception.Message);
        }

        [Fact]
        public async Task SetupTwoFactorAuthenticator_SuccessfulSetup_ReturnsResponse()
        {
            // Arrange
            var email = "testuser@example.com";
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Email = email,
                TwoFactorEnabled = false,
            };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(account.Id))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.AccountRepository.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            _twoFactorAuthenticatorMock.Setup(t => t.GenerateSetupCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(new SetupCode("ManualEntryKey", "ImageUrlQrCode", "QrCodeSetupImageUrl"));

            // Act
            var response = await _accountService.SetupTwoFactorAuthenticator(account.Id);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.ImageUrlQrCode);
            Assert.NotNull(response.ManualEntryKey);
            _unitOfWorkMock.Verify(u => u.AccountRepository.UpdateAsync(It.IsAny<Account>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAccountById_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(new List<Account>().AsQueryable().BuildMock());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.GetAccountById(accountId));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task GetAccountById_AccountFound_ReturnsViewAccount()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var role = new Role { RoleId = 1, RoleName = "Admin" };
            var account = new Account
            {
                Id = accountId,
                UserName = "testuser",
                RoleId = role.RoleId,
                Role = role
            };
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Configure Mapster mapping
            TypeAdapterConfig<Account, ViewAccount>.NewConfig()
                    .Map(dest => dest.RoleName, src => src.Role.RoleName);
            // Act
            var result = await _accountService.GetAccountById(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.Id);
            Assert.Equal("testuser", result.UserName);
            Assert.Equal("Admin", result.RoleName);
        }

        [Fact]
        public async Task UpdateAccount_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateAccountRequest();
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.UpdateAccount(accountId, request));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task UpdateAccount_SuccessfulUpdate_ReturnsBaseResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateAccountRequest
            {
                UserName = "newUsername",
                Email = "newEmail@example.com",
                FullName = "New FullName",
                PhoneNumber = "0987654321"
            };
            var account = new Account
            {
                Id = accountId,
                UserName = "existingUsername",
                Email = "existingEmail@example.com",
                FullName = "Existing FullName",
                PhoneNumber = "1234567890"
            };
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                           .Returns(Task.CompletedTask);

            // Act
            var response = await _accountService.UpdateAccount(accountId, request);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Cập nhật tài khoản thành công", response.Message);
            Assert.Equal(request.UserName, account.UserName);
            Assert.Equal(request.Email, account.Email);
            Assert.Equal(request.FullName, account.FullName);
            Assert.Equal(request.PhoneNumber, account.PhoneNumber);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAccountSettings_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateAccountSettingsRequest();
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.UpdateAccountSettings(accountId, request));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task UpdateAccountSettings_InvalidPreferredLanguage_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateAccountSettingsRequest
            {
                PreferredLanguage = "eng" // Invalid language code
            };
            var account = new Account
            {
                Id = accountId,
                AccountSettings = new AccountSettings()
            };
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync(account);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.UpdateAccountSettings(accountId, request));
            Assert.Equal("Ngôn ngữ không hợp lệ, vui lòng nhập mã ngôn ngữ 2 ký tự (ví dụ: 'en', 'vi')", exception.Message);
        }

        [Fact]
        public async Task UpdateAccountSettings_SuccessfulUpdate_ReturnsBaseResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateAccountSettingsRequest
            {
                PreferredLanguage = "en"
            };
            var account = new Account
            {
                Id = accountId,
                AccountSettings = new AccountSettings()
            };
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                           .Returns(Task.CompletedTask);

            // Act
            var response = await _accountService.UpdateAccountSettings(accountId, request);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Cập nhật cài đặt tài khoản thành công", response.Message);
            Assert.Equal(request.PreferredLanguage, account.AccountSettings.PreferredLanguage);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAccountsPaginatedAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var request = new QueryPaging
            {
                Page = 1,
                PageSize = 10,
                Search = "test"
            };

            var accounts = new List<Account>
            {
                new Account { UserName = "testuser1", Email = "test1@example.com", PhoneNumber = "1234567890", Status = AccountStatus.Active, Role = new Role { RoleName = "User" } },
                new Account { UserName = "testuser2", Email = "test2@example.com", PhoneNumber = "0987654321", Status = AccountStatus.Active, Role = new Role { RoleName = "Admin" } }
            }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetAll())
                           .Returns(accounts);

            // Act
            var result = await _accountService.GetAccountsPaginatedAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("testuser1", result.Items[0].UserName);
            Assert.Equal("testuser2", result.Items[1].UserName);
        }

        [Fact]
        public async Task GetAccountsPaginatedAsync_EmptySearch_ReturnsAllActiveAccounts()
        {
            // Arrange
            var request = new QueryPaging
            {
                Page = 1,
                PageSize = 10,
                Search = ""
            };

            var accounts = new List<Account>
            {
                new Account { UserName = "testuser1", Email = "test1@example.com", PhoneNumber = "1234567890", Status = AccountStatus.Active, Role = new Role { RoleName = "User" } },
                new Account { UserName = "testuser2", Email = "test2@example.com", PhoneNumber = "0987654321", Status = AccountStatus.Active, Role = new Role { RoleName = "Admin" } }
            }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetAll())
                           .Returns(accounts);

            // Act
            var result = await _accountService.GetAccountsPaginatedAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("testuser1", result.Items[0].UserName);
            Assert.Equal("testuser2", result.Items[1].UserName);
        }

        [Fact]
        public async Task GetAccountsPaginatedAsync_NoMatchingAccounts_ReturnsEmptyResult()
        {
            // Arrange
            var request = new QueryPaging
            {
                Page = 1,
                PageSize = 10,
                Search = "nonexistent"
            };

            var accounts = new List<Account>
            {
                new Account { UserName = "testuser1", Email = "test1@example.com", PhoneNumber = "1234567890", Status = AccountStatus.Active, Role = new Role { RoleName = "User" } },
                new Account { UserName = "testuser2", Email = "test2@example.com", PhoneNumber = "0987654321", Status = AccountStatus.Active, Role = new Role { RoleName = "Admin" } }
            }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetAll())
                           .Returns(accounts);

            // Act
            var result = await _accountService.GetAccountsPaginatedAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task DeactiveAccount_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.DeactiveAccount(accountId));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task DeactiveAccount_SuccessfulDeactivation_ReturnsBaseResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Status = AccountStatus.Active
            };
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                           .Returns(Task.CompletedTask);

            // Act
            var response = await _accountService.DeactiveAccount(accountId);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Tài khoản đã được vô hiệu hóa thành công", response.Message);
            Assert.Equal(AccountStatus.Inactive, account.Status);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAccount_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.DeleteAccount(accountId));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task DeleteAccount_SuccessfulDeletion_ReturnsBaseResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Status = AccountStatus.Active
            };
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                           .Returns(Task.CompletedTask);

            // Act
            var response = await _accountService.DeleteAccount(accountId);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Tài khoản đã được xóa thành công", response.Message);
            Assert.Equal(AccountStatus.Deleted, account.Status);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ActiveAccount_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.ActiveAccount(accountId));
            Assert.Equal("Không tìm thấy tài khoản", exception.Message);
        }

        [Fact]
        public async Task ActiveAccount_SuccessfulActivation_ReturnsBaseResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                Status = AccountStatus.Inactive
            };
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                           .Returns(Task.CompletedTask);

            // Act
            var response = await _accountService.ActiveAccount(accountId);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Tài khoản đã được kích hoạt thành công", response.Message);
            Assert.Equal(AccountStatus.Active, account.Status);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = "user@example.com";
            var mockAccounts = new List<Account>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.ResetPassword(accountId));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task ResetPassword_SuccessfulReset_ReturnsBaseResponse()
        {
            // Arrange
            var accountId = "test@example.com";
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserName = "testuser",
                Email = "test@example.com"
            };

            var mockAccounts = new List<Account>            {
                new Account { Id = Guid.NewGuid(), UserName = "testuser", Email = "test@example.com" },
            }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                           .Returns(Task.CompletedTask);
            _emailServiceMock.Setup(es => es.SendEmailAsync(account.Email, "Đặt lại mật khẩu", It.IsAny<string>()))
                             .Returns(Task.CompletedTask);

            // Act
            var response = await _accountService.ResetPassword(accountId);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Mật khẩu đã được đặt lại thành công, vui lòng kiểm tra email để biết thông tin đăng nhập", response.Message);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
            _emailServiceMock.Verify(es => es.SendEmailAsync(account.Email, "Đặt lại mật khẩu", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new ChangePasswordRequest { OldPassword = "oldPassword", NewPassword = "newPassword" };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId)).ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.ChangePassword(accountId, request));
            Assert.Equal("Không tìm thấy tài khoản", exception.Message);
        }

        [Fact]
        public async Task ChangePassword_OldPasswordIncorrect_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            account.PasswordHash = HashPassword(account, "hashedPassword");
            var request = new ChangePasswordRequest { OldPassword = "oldPassword", NewPassword = "newPassword" };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId)).ReturnsAsync(account);
            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(account, account.PasswordHash, request.OldPassword))
                               .Returns(PasswordVerificationResult.Failed);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.ChangePassword(accountId, request));
            Assert.Equal("Mật khẩu cũ không chính xác", exception.Message);
        }

        [Fact]
        public async Task ChangePassword_SuccessfulChange_ReturnsBaseResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            account.PasswordHash = HashPassword(account, "hashedPassword");
            var request = new ChangePasswordRequest { OldPassword = "oldPassword", NewPassword = "newPassword" };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId)).ReturnsAsync(account);
            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(account, account.PasswordHash, request.OldPassword))
                               .Returns(PasswordVerificationResult.Success);
            _passwordHelperMock.Setup(p => p.HashPassword(account, request.NewPassword)).Returns("newHashedPassword");

            // Act
            var response = await _accountService.ChangePassword(accountId, request);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Đổi mật khẩu thành công", response.Message);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.Equal("newHashedPassword", account.PasswordHash);
        }

        [Fact]
        public async Task ConfirmSetupTwoFactorAuthenticator_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new ConfirmSetupTwoFactorAuthenticatorRequest { OTPCode = "123456" };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId)).ReturnsAsync((Account)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _accountService.ConfirmSetupTwoFactorAuthenticator(accountId, request));
        }

        [Fact]
        public async Task ConfirmSetupTwoFactorAuthenticator_TwoFactorCodeIncorrect_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new ConfirmSetupTwoFactorAuthenticatorRequest { OTPCode = "123456" };
            var account = new Account { tOTPSecretKey = new byte[16] };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId)).ReturnsAsync(account);
            _twoFactorAuthenticatorMock.Setup(t => t.ValidateTwoFactorPIN(account.tOTPSecretKey, request.OTPCode.Trim(), 0)).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _accountService.ConfirmSetupTwoFactorAuthenticator(accountId, request));
        }

        [Fact]
        public async Task ConfirmSetupTwoFactorAuthenticator_TwoFactorCodeAlreadyUsed_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new ConfirmSetupTwoFactorAuthenticatorRequest { OTPCode = "123456" };
            var account = new Account { tOTPSecretKey = new byte[16], OTPCode = Utils.Base64Encode("123456") };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId)).ReturnsAsync(account);
            _twoFactorAuthenticatorMock.Setup(t => t.ValidateTwoFactorPIN(account.tOTPSecretKey, request.OTPCode.Trim(), 0)).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _accountService.ConfirmSetupTwoFactorAuthenticator(accountId, request));
        }

        [Fact]
        public async Task ConfirmSetupTwoFactorAuthenticator_SuccessfulConfirmation_ReturnsBaseResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new ConfirmSetupTwoFactorAuthenticatorRequest { OTPCode = "123456" };
            var account = new Account { tOTPSecretKey = new byte[16] };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId)).ReturnsAsync(account);
            _twoFactorAuthenticatorMock.Setup(t => t.ValidateTwoFactorPIN(account.tOTPSecretKey, request.OTPCode.Trim(), 0)).Returns(true);

            // Act
            var result = await _accountService.ConfirmSetupTwoFactorAuthenticator(accountId, request);

            // Assert
            Assert.Equal(200, result.Code);
            Assert.Equal("Xác thực 2FA thành công", result.Message);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        private string HashPassword(Account account, string password)
        {
            return _passwordHasher.HashPassword(account, password);
        }
    }
}