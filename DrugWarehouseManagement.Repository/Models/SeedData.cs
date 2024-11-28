using DrugWarehouseManagement.Common.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    RoleName = "Inventory Staff"
                });

            modelBuilder.Entity<Account>().HasData(
                // Admin Accounts
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "admin1",
                    Email = "admin1@example.com",
                    FullName = "Admin One",
                    PhoneNumber = "1234567890",
                    Password = HashPassword("SecurePassword1!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 1 // Admin
                },
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "admin2",
                    Email = "admin2@example.com",
                    FullName = "Admin Two",
                    PhoneNumber = "0987654321",
                    Password = HashPassword("SecurePassword2!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 1 // Admin
                },
                // Inventory Manager Accounts
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "manager1",
                    Email = "manager1@example.com",
                    FullName = "Manager One",
                    PhoneNumber = "1122334455",
                    Password = HashPassword("SecurePassword3!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 2 // Inventory Manager
                },
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "manager2",
                    Email = "manager2@example.com",
                    FullName = "Manager Two",
                    PhoneNumber = "5566778899",
                    Password = HashPassword("SecurePassword4!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 2 // Inventory Manager
                },
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "manager3",
                    Email = "manager3@example.com",
                    FullName = "Manager Three",
                    PhoneNumber = "6677889900",
                    Password = HashPassword("SecurePassword5!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 2 // Inventory Manager
                },
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "manager4",
                    Email = "manager4@example.com",
                    FullName = "Manager Four",
                    PhoneNumber = "3344556677",
                    Password = HashPassword("SecurePassword6!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 2 // Inventory Manager
                },
                // Inventory Staff Accounts
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "staff1",
                    Email = "staff1@example.com",
                    FullName = "Staff One",
                    PhoneNumber = "7788990011",
                    Password = HashPassword("SecurePassword7!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 3 // Inventory Staff
                },
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "staff2",
                    Email = "staff2@example.com",
                    FullName = "Staff Two",
                    PhoneNumber = "9900112233",
                    Password = HashPassword("SecurePassword8!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 3 // Inventory Staff
                },
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "staff3",
                    Email = "staff3@example.com",
                    FullName = "Staff Three",
                    PhoneNumber = "2233445566",
                    Password = HashPassword("SecurePassword9!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 3 // Inventory Staff
                },
                new Account
                {
                    AccountId = Guid.NewGuid(),
                    Username = "staff4",
                    Email = "staff4@example.com",
                    FullName = "Staff Four",
                    PhoneNumber = "4455667788",
                    Password = HashPassword("SecurePassword10!"),
                    CreatedAt = DateTime.Now,
                    Status = Status.Active,
                    RoleId = 3 // Inventory Staff
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    Name = "Paracetamol",
                    Code = "PRC001",
                    SKU = "PARA-500-TAB",
                    Description = "Pain reliever and fever reducer",
                    Categories = "Medicine",
                    UnitOfMeasure = "Box",
                    Supplier = "PharmaCorp",
                    ApprovalStatus = "Approved",
                    ImageUrl = "https://example.com/paracetamol.jpg",
                    Instructions = "Take one tablet every 6 hours after meals",
                    StorageCondition = "Store in a cool, dry place",
                    SideEffects = "Drowsiness, nausea",
                    ExpiryDate = DateTime.Now.AddYears(2),
                    ReorderPoint = DateTime.Now.AddMonths(-1), // Example for testing
                    ReorderQuantity = 100,
                    CreatedAt = DateTime.Now,
                    Status = Status.Active
                },
                new Product
                {
                    ProductId = 2,
                    Name = "Ibuprofen",
                    Code = "PRC002",
                    Description = "Nonsteroidal anti-inflammatory drug",
                    SKU = "IBU-400-CAP-20",
                    Categories = "Medicine",
                    UnitOfMeasure = "Bottle",
                    Supplier = "HealthPlus",
                    ApprovalStatus = "Approved",
                    ImageUrl = "https://example.com/ibuprofen.jpg",
                    Instructions = "Take one capsule every 8 hours with water",
                    StorageCondition = "Keep at room temperature",
                    SideEffects = "Stomach upset, dizziness",
                    ExpiryDate = DateTime.Now.AddYears(1),
                    ReorderPoint = DateTime.Now.AddMonths(-2), // Example for testing
                    ReorderQuantity = 50,
                    CreatedAt = DateTime.Now,
                    Status = Status.Active
                }
            );

            modelBuilder.Entity<Drug>().HasData(
                new Drug
                {
                    DrugId = 1,
                    ProductId = 1,
                    TemperatureRange = "15°C - 30°C",
                    HumidityRange = "30% - 50%",
                    IsLightSensitive = false,
                    CreatedAt = DateTime.Now,
                    Status = Status.Active
                },
                new Drug
                {
                    DrugId = 2,
                    ProductId = 2,
                    TemperatureRange = "15°C - 25°C",
                    HumidityRange = "40% - 60%",
                    IsLightSensitive = true, // As Ibuprofen is light-sensitive in some forms
                    CreatedAt = DateTime.Now,
                    Status = Status.Active
                }
            );

        }

        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }
    }
}
