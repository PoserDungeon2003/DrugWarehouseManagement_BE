using DrugWarehouseManagement.Common.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace DrugWarehouseManagement.Repository.Models
{
    public class SeedData
    {
        private readonly ModelBuilder modelBuilder;
        private readonly PasswordHasher<string> _passwordHasher;

        public SeedData(ModelBuilder modelBuilder)
        {
            this.modelBuilder = modelBuilder;
            _passwordHasher = new PasswordHasher<string>();
        }

        public void Seed()
        {
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = 1,
                    RoleName = "Admin"
                },
                new Role
                {
                    RoleId = 2,
                    RoleName = "Inventory Manager"
                },
                new Role
                {
                    RoleId = 3,
                    RoleName = "Accountant"
                },
                new Role
                {
                    RoleId = 4,
                    RoleName = "Sale Admin"
                });

            modelBuilder.Entity<Account>().HasData(
                // Admin Accounts
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "admin1",
                    Email = "admin1@example.com",
                    FullName = "Admin One",
                    PhoneNumber = "1234567890",
                    PasswordHash = HashPassword("SecurePassword1!"),
                    Status = AccountStatus.Active,
                    RoleId = 1 // Admin
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "admin2",
                    Email = "admin2@example.com",
                    FullName = "Admin Two",
                    PhoneNumber = "0987654321",
                    PasswordHash = HashPassword("SecurePassword2!"),
                    Status = AccountStatus.Active,
                    RoleId = 1 // Admin
                },
                // Inventory Manager Accounts
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "manager1",
                    Email = "manager1@example.com",
                    FullName = "Manager One",
                    PhoneNumber = "1122334455",
                    PasswordHash = HashPassword("SecurePassword3!"),
                    Status = AccountStatus.Active,
                    RoleId = 2 // Inventory Manager
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "manager2",
                    Email = "manager2@example.com",
                    FullName = "Manager Two",
                    PhoneNumber = "5566778899",
                    PasswordHash = HashPassword("SecurePassword4!"),
                    Status = AccountStatus.Active,
                    RoleId = 2 // Inventory Manager
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "manager3",
                    Email = "manager3@example.com",
                    FullName = "Manager Three",
                    PhoneNumber = "6677889900",
                    PasswordHash = HashPassword("SecurePassword5!"),
                    Status = AccountStatus.Active,
                    RoleId = 2 // Inventory Manager
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "manager4",
                    Email = "manager4@example.com",
                    FullName = "Manager Four",
                    PhoneNumber = "3344556677",
                    PasswordHash = HashPassword("SecurePassword6!"),
                    Status = AccountStatus.Active,
                    RoleId = 2 // Inventory Manager
                },
                // Inventory Staff Accounts
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "staff1",
                    Email = "staff1@example.com",
                    FullName = "Staff One",
                    PhoneNumber = "7788990011",
                    PasswordHash = HashPassword("SecurePassword7!"),
                    Status = AccountStatus.Active,
                    RoleId = 3 // Inventory Staff
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "staff2",
                    Email = "staff2@example.com",
                    FullName = "Staff Two",
                    PhoneNumber = "9900112233",
                    PasswordHash = HashPassword("SecurePassword8!"),
                    Status = AccountStatus.Active,
                    RoleId = 3 // Inventory Staff
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "staff3",
                    Email = "staff3@example.com",
                    FullName = "Staff Three",
                    PhoneNumber = "2233445566",
                    PasswordHash = HashPassword("SecurePassword9!"),
                    Status = AccountStatus.Active,
                    RoleId = 3 // Inventory Staff
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    UserName = "staff4",
                    Email = "staff4@example.com",
                    FullName = "Staff Four",
                    PhoneNumber = "4455667788",
                    PasswordHash = HashPassword("SecurePassword10!"),
                    Status = AccountStatus.Active,
                    RoleId = 3 // Inventory Staff
                }
            );
        }

        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }
    }
}
