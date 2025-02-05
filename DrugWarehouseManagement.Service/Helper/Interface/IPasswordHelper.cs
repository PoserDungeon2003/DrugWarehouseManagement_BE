using DrugWarehouseManagement.Repository.Models;
using Microsoft.AspNetCore.Identity;

namespace DrugWarehouseManagement.Service.Helper.Interface
{
    public interface IPasswordHelper
    {
        string HashPassword(Account account, string password);
        PasswordVerificationResult VerifyHashedPassword(Account account, string hashedPassword, string password);
    }
}
