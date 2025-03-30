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

        public AccountServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _passwordHelperMock = new Mock<IPasswordWrapper>();
            _tokenHandlerMock = new Mock<ITokenHandlerService>();
            _twoFactorAuthenticatorMock = new Mock<ITwoFactorAuthenticatorWrapper>();
            _loggerMock = new Mock<ILogger<IAccountService>>();
            _emailServiceMock = new Mock<IEmailService>();
            _passwordHasher ??= new PasswordHasher<Account>();

            _accountService = new AccountService(
                _unitOfWorkMock.Object,
                _tokenHandlerMock.Object,
                _loggerMock.Object,
                _emailServiceMock.Object,
                _twoFactorAuthenticatorMock.Object,
                _passwordHelperMock.Object
            );
        }

        [Fact]
        public async Task LoginWithUsername_AccountNotFound_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "testuser", Password = "password" };
            var mockAccounts = new List<Account>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.LoginWithUsername(request));
            Assert.Equal("Account not found", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_AccountInactive_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "testuser", Password = "password" };
            var account = new Account { Status = AccountStatus.Inactive };
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.LoginWithUsername(request));
            Assert.Equal("Account is inactive, please contact your administrator to re-active your account", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_TwoFactorCodeRequired_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "testuser", Password = "password" };
            var account = new Account
            {
                UserName = "testuser",
                Status = AccountStatus.Active,
                TwoFactorEnabled = true,
            };
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.LoginWithUsername(request));
            Assert.Equal("Two factor code is required", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_TwoFactorCodeIncorrect_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "testuser", Password = "password", OTPCode = "123456" };
            var account = new Account
            {
                Status = AccountStatus.Active,
                TwoFactorEnabled = true,
                tOTPSecretKey = new byte[16],
            };
            account.PasswordHash = HashPassword(account, "password");
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            _twoFactorAuthenticatorMock.Setup(t => t.ValidateTwoFactorPIN(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.LoginWithUsername(request));
            Assert.Equal("Two factor code is incorrect", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_TwoFactorCodeUsed_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "testuser", Password = "password", OTPCode = "123456" };
            var account = new Account
            {
                Status = AccountStatus.Active,
                TwoFactorEnabled = true,
                tOTPSecretKey = new byte[16],
                OTPCode = Utils.Base64Encode("123456")
            };
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            _twoFactorAuthenticatorMock.Setup(t => t.ValidateTwoFactorPIN(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.LoginWithUsername(request));
            Assert.Equal("Two factor code is already used", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_PasswordIncorrect_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "testuser", Password = "password" };
            var account = new Account
            {
                Status = AccountStatus.Active,
            };
            account.PasswordHash = HashPassword(account, "hashedpassword");
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Failed);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.LoginWithUsername(request));
            Assert.Equal("Password is incorrect", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_SuccessfulLogin_ReturnsResponse()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "testuser", Password = "hashedpassword" };
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserName = "testuser",
                Status = AccountStatus.Active,
                Role = new Role { RoleId = 1, RoleName = "Role" },
                RoleId = 1,
            };
            account.PasswordHash = HashPassword(account, "hashedpassword");
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Account());
            _passwordHelperMock.Setup(p => p.VerifyHashedPassword(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Success);
            _tokenHandlerMock.Setup(t => t.GenerateJwtToken(It.IsAny<Account>()))
                .Returns("Token");

            // Act
            var result = await _accountService.LoginWithUsername(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Role", result.Role);
            Assert.Equal("Token", result.Token);
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
            Assert.Equal("Account already existed", exception.Message);
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
            Assert.Equal("Account created successfully, please check your (spam) inbox for login credentials", response.Message);
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
            Assert.Equal("Account not found", exception.Message);
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
            Assert.Equal("Two factor authenticator is already setup", exception.Message);
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
            Assert.Equal("Account not found", exception.Message);
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
            Assert.Equal("Account not found", exception.Message);
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
            Assert.Equal("Account updated successfully", response.Message);
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
            Assert.Equal("Account not found", exception.Message);
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
            Assert.Equal("Preferred language must be exactly 2 alphabetic characters", exception.Message);
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
            Assert.Equal("Account settings updated successfully", response.Message);
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
            Assert.Equal("Account not found", exception.Message);
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
            Assert.Equal("Account deactivated successfully", response.Message);
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
            Assert.Equal("Account not found", exception.Message);
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
            Assert.Equal("Account deleted successfully", response.Message);
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
            Assert.Equal("Account not found", exception.Message);
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
            Assert.Equal("Account activated successfully", response.Message);
            Assert.Equal(AccountStatus.Active, account.Status);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.ResetPassword(accountId));
            Assert.Equal("Account not found", exception.Message);
        }

        [Fact]
        public async Task ResetPassword_SuccessfulReset_ReturnsBaseResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                UserName = "testuser",
                Email = "test@example.com"
            };

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                           .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                           .Returns(Task.CompletedTask);
            _emailServiceMock.Setup(es => es.SendEmailAsync(account.Email, "Reset Password", It.IsAny<string>()))
                             .Returns(Task.CompletedTask);

            // Act
            var response = await _accountService.ResetPassword(accountId);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Password reset successfully, please check your (spam) inbox for new login credentials", response.Message);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
            _emailServiceMock.Verify(es => es.SendEmailAsync(account.Email, "Reset Password", It.IsAny<string>()), Times.Once);
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
            Assert.Equal("Account not found", exception.Message);
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
            Assert.Equal("Old password is incorrect", exception.Message);
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
            Assert.Equal("Password changed successfully", response.Message);
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
            Assert.Equal("Two factor authenticator confirmed successfully", result.Message);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        private string HashPassword(Account account, string password)
        {
            return _passwordHasher.HashPassword(account, password);
        }
    }
}