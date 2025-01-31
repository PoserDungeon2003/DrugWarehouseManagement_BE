using DrugWarehouseManagement.Repository.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Helper.Interface
{
    public interface IPasswordHelper
    {
        string HashPassword(Account account, string password);
        PasswordVerificationResult VerifyHashedPassword(Account account, string hashedPassword, string password);
    }
}
