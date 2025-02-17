using DrugWarehouseManagement.Repository.Models;
using Microsoft.AspNetCore.Identity;

namespace DrugWarehouseManagement.Service.Wrapper.Interface
{
    public interface IPasswordWrapper
    {
        string HashPassword(Account account, string password);
        PasswordVerificationResult VerifyHashedPassword(Account account, string hashedPassword, string password);
        string HashValue(string value);
        PasswordVerificationResult VerifyHashedValue(string hashedValue, string value);
    }
}
