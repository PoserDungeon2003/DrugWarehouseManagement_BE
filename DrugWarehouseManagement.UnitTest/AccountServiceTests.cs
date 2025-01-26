using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Request;
using DrugWarehouseManagement.Service.Services;
using DrugWarehouseManagement.Service.Wrapper.Interface;
using Google.Authenticator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MockQueryable;
using Moq;
using NodaTime;
using System;
using System.Data;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace DrugWarehouseManagement.UnitTest
{
    public class AccountServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPasswordHasher<string>> _passwordHasherMock;
        private readonly Mock<ITokenHandlerService> _tokenHandlerMock;
        private readonly Mock<ITwoFactorAuthenticatorWrapper> _twoFactorAuthenticatorMock;
        private readonly Mock<ILogger<IAccountService>> _loggerMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly AccountService _accountService;
        private readonly IPasswordHasher<string> _passwordHasher;

        public AccountServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _passwordHasherMock = new Mock<IPasswordHasher<string>>();
            _tokenHandlerMock = new Mock<ITokenHandlerService>();
            _twoFactorAuthenticatorMock = new Mock<ITwoFactorAuthenticatorWrapper>();
            _loggerMock = new Mock<ILogger<IAccountService>>();
            _emailServiceMock = new Mock<IEmailService>();
            _passwordHasher = new PasswordHasher<string>();

            _accountService = new AccountService(
                _unitOfWorkMock.Object,
                _tokenHandlerMock.Object,
                _loggerMock.Object,
                _emailServiceMock.Object,
                _twoFactorAuthenticatorMock.Object
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
            var account = new Account { Status = Common.Enums.AccountStatus.Inactive };
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
                Username = "testuser",
                Status = Common.Enums.AccountStatus.Active,
                AccountSettings = new AccountSettings { IsTwoFactorEnabled = true }
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
            var request = new AccountLoginRequest { Username = "testuser", Password = "password", tOtpCode = "123456" };
            var account = new Account
            {
                Status = Common.Enums.AccountStatus.Active,
                AccountSettings = new AccountSettings { IsTwoFactorEnabled = true },
                tOTPSecretKey = new byte[16]
            };
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            //_twoFactorAuthenticatorMock.Setup(t => t.ValidateTwoFactorPIN(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<int>()))
            //    .Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.LoginWithUsername(request));
            Assert.Equal("Two factor code is incorrect", exception.Message);
        }

        [Fact]
        public async Task LoginWithUsername_TwoFactorCodeUsed_ThrowsException()
        {
            // Arrange
            var request = new AccountLoginRequest { Username = "testuser", Password = "password", tOtpCode = "123456" };
            var account = new Account
            {
                Status = Common.Enums.AccountStatus.Active,
                AccountSettings = new AccountSettings { IsTwoFactorEnabled = true },
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
                Status = Common.Enums.AccountStatus.Active,
                Password = HashPassword("hashedpassword"),
            };
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            _passwordHasherMock.Setup(p => p.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
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
                AccountId = Guid.NewGuid(),
                Username = "testuser",
                Status = Common.Enums.AccountStatus.Active,
                Password = HashPassword("hashedpassword"),
                Role = new Role { RoleId = 1, RoleName = "Role" },
                RoleId = 1,
            };
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Account());
            _passwordHasherMock.Setup(p => p.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
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
        public async Task UpdateLastLogin_AccountNotFound_ThrowsException()
        {
            // Arrange
            var updateLastLoginDTO = new UpdateLastLoginDTO { AccountId = Guid.NewGuid(), LastLogin = SystemClock.Instance.GetCurrentInstant() };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.UpdateLastLogin(updateLastLoginDTO));
            Assert.Equal("Account not found", exception.Message);
        }

        [Fact]
        public async Task UpdateLastLogin_SuccessfulUpdate()
        {
            // Arrange
            var account = new Account { AccountId = Guid.NewGuid() };
            var updateLastLoginDTO = new UpdateLastLoginDTO { AccountId = account.AccountId, LastLogin = SystemClock.Instance.GetCurrentInstant() };
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.AccountRepository.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            await _accountService.UpdateLastLogin(updateLastLoginDTO);

            // Assert
            _unitOfWorkMock.Verify(u => u.AccountRepository.UpdateAsync(It.Is<Account>(a => a.AccountId == account.AccountId)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAccount_AccountAlreadyExists_ThrowsException()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                Username = "testuser",
                Email = "testuser@example.com",
                PhoneNumber = "1234567890"
            };
            var existingAccount = new Account { Username = "testuser" };
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
                Username = "newuser",
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
            var email = "testuser@example.com";
            var mockAccounts = new List<Account>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.SetupTwoFactorAuthenticator(email));
            Assert.Equal("Account not found", exception.Message);
        }

        [Fact]
        public async Task SetupTwoFactorAuthenticator_AlreadySetup_ThrowsException()
        {
            // Arrange
            var email = "testuser@example.com";
            var account = new Account
            {
                Email = email,
                AccountSettings = new AccountSettings { IsTwoFactorEnabled = true }
            };
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _accountService.SetupTwoFactorAuthenticator(email));
            Assert.Equal("Two factor authenticator is already setup", exception.Message);
        }

        [Fact]
        public async Task SetupTwoFactorAuthenticator_SuccessfulSetup_ReturnsResponse()
        {
            // Arrange
            var email = "testuser@example.com";
            var account = new Account
            {
                Email = email,
                AccountSettings = new AccountSettings { IsTwoFactorEnabled = false }
            };
            var mockAccounts = new List<Account> { account }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByWhere(It.IsAny<Expression<Func<Account, bool>>>()))
                .Returns(mockAccounts);
            _unitOfWorkMock.Setup(u => u.AccountRepository.UpdateAsync(It.IsAny<Account>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);
            _twoFactorAuthenticatorMock.Setup(t => t.GenerateSetupCode(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Returns(new SetupCode("ManualEntryKey", "ImageUrlQrCode", "QrCodeSetupImageUrl"));

            // Act
            var response = await _accountService.SetupTwoFactorAuthenticator(email);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.ImageUrlQrCode);
            Assert.NotNull(response.ManualEntryKey);
            _unitOfWorkMock.Verify(u => u.AccountRepository.UpdateAsync(It.IsAny<Account>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }
    }
}