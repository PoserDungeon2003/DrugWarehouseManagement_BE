using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.Wrapper.Interface;
using Microsoft.AspNetCore.Identity;

namespace DrugWarehouseManagement.Service.Wrapper
{
    public class PasswordWrapper : IPasswordWrapper
    {
        private readonly IPasswordHasher<Account> _passwordHelper;

        public PasswordWrapper(IPasswordHasher<Account> passwordHelper)
        {
            _passwordHelper = passwordHelper;
        }

        public string HashPassword(Account account, string password)
            => _passwordHelper.HashPassword(account, password);

        public string HashValue(string value)
            => _passwordHelper.HashPassword(null, value);

        public PasswordVerificationResult VerifyHashedPassword(Account account, string hashedPassword, string password)
            => _passwordHelper.VerifyHashedPassword(account, hashedPassword, password);

        public PasswordVerificationResult VerifyHashedValue(string hashedValue, string value)
            => _passwordHelper.VerifyHashedPassword(null, hashedValue, value);
    }
}
