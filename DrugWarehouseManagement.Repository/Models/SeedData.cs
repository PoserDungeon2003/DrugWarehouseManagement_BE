using DrugWarehouseManagement.Common.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            var instantNow = SystemClock.Instance.GetCurrentInstant();

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Inventory Manager" },
                new Role { RoleId = 3, RoleName = "Accountant" },
                new Role { RoleId = 4, RoleName = "Sale Admin" }
            );

            // Seed Accounts with fixed GUIDs for those that will be referenced by Outbounds
            modelBuilder.Entity<Account>().HasData(
                // Admin Accounts
                new Account
                {
                    Id = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"),
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
                    Id = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"),
                    UserName = "admin2",
                    Email = "admin2@example.com",
                    FullName = "Admin Two",
                    PhoneNumber = "0987654321",
                    PasswordHash = HashPassword("SecurePassword2!"),
                    Status = AccountStatus.Active,
                    RoleId = 1 // Admin
                },
                // Inventory Manager Accounts (these IDs will be used in Outbounds)
                new Account
                {
                    Id = Guid.Parse("7e006221-9a70-498d-a0b2-ae587c0cd1e8"),
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
                    Id = Guid.Parse("4cab1ddc-9ebf-4488-aa28-c472393623ac"),
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
                    Id = Guid.Parse("88376119-6a82-489f-97d4-8b2ad19b7d67"),
                    UserName = "manager3",
                    Email = "manager3@example.com",
                    FullName = "Manager Three",
                    PhoneNumber = "6677889900",
                    PasswordHash = HashPassword("SecurePassword5!"),
                    Status = AccountStatus.Active,
                    RoleId = 2 // Inventory Manager
                },
                // You may add additional accounts here with new GUIDs if needed.
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

            // Seed Providers
            modelBuilder.Entity<Provider>().HasData(
                new Provider { ProviderId = 1, ProviderName = "ABC Pharma", Address = "12 Pharma St", PhoneNumber = "123456789", TaxCode = "TX123", Email = "abc@pharma.com", Status = ProviderStatus.Active },
                new Provider { ProviderId = 2, ProviderName = "XYZ Drugs", Address = "34 Medicine Rd", PhoneNumber = "987654321", TaxCode = "TX456", Email = "xyz@drugs.com", Status = ProviderStatus.Active },
                new Provider { ProviderId = 3, ProviderName = "MediCorp", Address = "56 Health Ave", PhoneNumber = "555555555", TaxCode = "TX789", Email = "contact@medicorp.com", Status = ProviderStatus.Active },
                new Provider { ProviderId = 4, ProviderName = "Wellness Inc", Address = "78 Wellness Ln", PhoneNumber = "111111111", TaxCode = "TX101", Email = "info@wellness.com", Status = ProviderStatus.Inactive },
                new Provider { ProviderId = 5, ProviderName = "SafeMeds", Address = "90 Secure Blvd", PhoneNumber = "222222222", TaxCode = "TX202", Email = "help@safemeds.com", Status = ProviderStatus.Active }
            );

            // Seed Warehouses
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { WarehouseId = 1, WarehouseName = "Central Warehouse", Address = "123 Main St", Status = WarehouseStatus.Active },
                new Warehouse { WarehouseId = 2, WarehouseName = "East Warehouse", Address = "456 East St", Status = WarehouseStatus.Inactive },
                new Warehouse { WarehouseId = 3, WarehouseName = "West Warehouse", Address = "789 West St", Status = WarehouseStatus.Inactive },
                new Warehouse { WarehouseId = 4, WarehouseName = "North Warehouse", Address = "321 North St", Status = WarehouseStatus.Active },
                new Warehouse { WarehouseId = 5, WarehouseName = "South Warehouse", Address = "654 South St", Status = WarehouseStatus.Active }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, ProductName = "Paracetamol", ProductCode = "P001", Type = "Tablet", MadeFrom = "Chemical", ProviderId = 1 },
                new Product { ProductId = 2, ProductName = "Aspirin", ProductCode = "P002", Type = "Capsule", MadeFrom = "Chemical", ProviderId = 2 },
                new Product { ProductId = 3, ProductName = "Ibuprofen", ProductCode = "P003", Type = "Gel", MadeFrom = "Chemical", ProviderId = 3 },
                new Product { ProductId = 4, ProductName = "Vitamin C", ProductCode = "P004", Type = "Syrup", MadeFrom = "Natural", ProviderId = 4 },
                new Product { ProductId = 5, ProductName = "Cough Syrup", ProductCode = "P005", Type = "Liquid", MadeFrom = "Herbal", ProviderId = 5 }
            );

            // Seed Lots
            modelBuilder.Entity<Lot>().HasData(
                new Lot { LotId = 1, LotNumber = "L001", Quantity = 100, TemporaryWarehouse = 10, ExpiryDate = instantNow.Plus(Duration.FromDays(365)), WarehouseId = 1, ProductId = 1 },
                new Lot { LotId = 2, LotNumber = "L002", Quantity = 200, TemporaryWarehouse = 20, ExpiryDate = instantNow.Plus(Duration.FromDays(300)), WarehouseId = 2, ProductId = 2 },
                new Lot { LotId = 3, LotNumber = "L003", Quantity = 150, TemporaryWarehouse = 15, ExpiryDate = instantNow.Plus(Duration.FromDays(180)), WarehouseId = 3, ProductId = 3 },
                new Lot { LotId = 4, LotNumber = "L004", Quantity = 250, TemporaryWarehouse = 25, ExpiryDate = instantNow.Plus(Duration.FromDays(450)), WarehouseId = 4, ProductId = 4 },
                new Lot { LotId = 5, LotNumber = "L005", Quantity = 300, TemporaryWarehouse = 30, ExpiryDate = instantNow.Plus(Duration.FromDays(500)), WarehouseId = 5, ProductId = 5 }
            );

            // Seed Outbounds referencing the same Account IDs as above
            modelBuilder.Entity<Outbound>().HasData(
                new Outbound
                {
                    OutboundId = 1,
                    OutboundCode = "OB001",
                    CustomerName = "John Doe",
                    Address = "100 Main St",
                    PhoneNumber = "1234567890",
                    OutboundOrderCode = "ORD001",
                    TrackingNumber = "TRK001",
                    OutboundDate = instantNow,
                    Status = OutboundStatus.Pending,
                    AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b")
                },
                new Outbound
                {
                    OutboundId = 2,
                    OutboundCode = "OB002",
                    CustomerName = "Jane Smith",
                    Address = "200 South St",
                    PhoneNumber = "9876543210",
                    OutboundOrderCode = "ORD002",
                    TrackingNumber = "TRK002",
                    OutboundDate = instantNow,
                    Status = OutboundStatus.InProgress,
                    AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7")
                },
                new Outbound
                {
                    OutboundId = 3,
                    OutboundCode = "OB003",
                    CustomerName = "Alice Brown",
                    Address = "300 West St",
                    PhoneNumber = "5551234567",
                    OutboundOrderCode = "ORD003",
                    TrackingNumber = "TRK003",
                    OutboundDate = instantNow,
                    Status = OutboundStatus.Packed,
                    AccountId = Guid.Parse("7e006221-9a70-498d-a0b2-ae587c0cd1e8")
                },
                new Outbound
                {
                    OutboundId = 4,
                    OutboundCode = "OB004",
                    CustomerName = "Bob Martin",
                    Address = "400 East St",
                    PhoneNumber = "4449876543",
                    OutboundOrderCode = "ORD004",
                    TrackingNumber = "TRK004",
                    OutboundDate = instantNow,
                    Status = OutboundStatus.AcceptedFromCustomer,
                    AccountId = Guid.Parse("4cab1ddc-9ebf-4488-aa28-c472393623ac")
                },
                new Outbound
                {
                    OutboundId = 5,
                    OutboundCode = "OB005",
                    CustomerName = "Charlie Davis",
                    Address = "500 North St",
                    PhoneNumber = "6661237890",
                    OutboundOrderCode = "ORD005",
                    TrackingNumber = "TRK005",
                    OutboundDate = instantNow,
                    Status = OutboundStatus.Cancelled,
                    AccountId = Guid.Parse("88376119-6a82-489f-97d4-8b2ad19b7d67")
                }
            );

            // Seed OutboundDetails
            modelBuilder.Entity<OutboundDetails>().HasData(
                new OutboundDetails
                {
                    OutboundDetailsId = 1,
                    LotNumber = "LOT-20240101",
                    ExpiryDate = instantNow.Plus(Duration.FromDays(365)),
                    Quantity = 50,
                    UnitType = "Box",
                    UnitPrice = 12.5m,
                    TotalPrice = 625m,
                    ProductId = 1,
                    OutboundId = 1,
                    LotId = 1
                },
                new OutboundDetails
                {
                    OutboundDetailsId = 2,
                    LotNumber = "LOT-20240201",
                    ExpiryDate = instantNow.Plus(Duration.FromDays(400)),
                    Quantity = 30,
                    UnitType = "Bottle",
                    UnitPrice = 20m,
                    TotalPrice = 600m,
                    ProductId = 2,
                    OutboundId = 2,
                    LotId = 2
                },
                new OutboundDetails
                {
                    OutboundDetailsId = 3,
                    LotNumber = "LOT-20240301",
                    ExpiryDate = instantNow.Plus(Duration.FromDays(450)),
                    Quantity = 40,
                    UnitType = "Pack",
                    UnitPrice = 8m,
                    TotalPrice = 320m,
                    ProductId = 3,
                    OutboundId = 3,
                    LotId = 3
                },
                new OutboundDetails
                {
                    OutboundDetailsId = 4,
                    LotNumber = "LOT-20240401",
                    ExpiryDate = instantNow.Plus(Duration.FromDays(500)),
                    Quantity = 20,
                    UnitType = "Vial",
                    UnitPrice = 50m,
                    TotalPrice = 1000m,
                    ProductId = 4,
                    OutboundId = 4,
                    LotId = 4
                },
                new OutboundDetails
                {
                    OutboundDetailsId = 5,
                    LotNumber = "LOT-20240501",
                    ExpiryDate = instantNow.Plus(Duration.FromDays(550)),
                    Quantity = 25,
                    UnitType = "Tube",
                    UnitPrice = 15m,
                    TotalPrice = 375m,
                    ProductId = 5,
                    OutboundId = 5,
                    LotId = 5
                }
            );
        }

        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword("dummy", password);
        }
    }
}
