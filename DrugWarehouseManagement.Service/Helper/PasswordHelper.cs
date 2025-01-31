using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.Helper.Interface;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Helper
{
    public class PasswordHelper : IPasswordHelper
    {
        private readonly IPasswordHasher<Account> _passwordHelper;

        public PasswordHelper(IPasswordHasher<Account> passwordHelper)
        {
            _passwordHelper = passwordHelper;
        }

        public string HashPassword(Account account, string password)
            => _passwordHelper.HashPassword(account, password);

        public PasswordVerificationResult VerifyHashedPassword(Account account, string hashedPassword, string password)
            => _passwordHelper.VerifyHashedPassword(account, hashedPassword, password);
    }
}
