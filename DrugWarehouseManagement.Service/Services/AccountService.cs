using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly TokenHandlerService _tokenHandler;

        public AccountService(IUnitOfWork unitOfWork, TokenHandlerService tokenHandler)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = new PasswordHasher<string>();
            _tokenHandler = tokenHandler;
        }

        public async Task<AccountLoginResponse> LoginWithEmail(AccountLoginRequest request)
        {
            var account = await _unitOfWork.AccountRepository
                        .GetByWhere(x => x.Email == request.Email)
                        .Include(x => x.Role)
                        .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new Exception("Email not found");
            }

            if (account.Status == Common.Enums.Status.Inactive)
            {
                throw new Exception("Account is inactive, please re-active your account");
            }

            var verifyPassword = _passwordHasher.VerifyHashedPassword(null, account.Password, request.Password);

            if (verifyPassword == PasswordVerificationResult.Failed)
            {
                throw new Exception("Password is incorrect");
            }

            return new AccountLoginResponse
            {
                Role = account.Role.RoleName,
                Token = _tokenHandler.GenerateJwtToken(account)
            };
        }
    }
}
