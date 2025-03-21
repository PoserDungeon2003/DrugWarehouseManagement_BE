using DrugWarehouseManagement.Common;
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

            // 1. Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Inventory Manager" },
                new Role { RoleId = 3, RoleName = "Accountant" },
                new Role { RoleId = 4, RoleName = "Sale Admin" },
                new Role { RoleId = 5, RoleName = "Director" }
            );

            // 2. Seed Accounts
            modelBuilder.Entity<Account>().HasData(
                new Account { Id = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), UserName = "admin1", Email = "admin1@example.com", FullName = "Admin One", PhoneNumber = "1234567890", PasswordHash = HashPassword("SecurePassword1!"), Status = AccountStatus.Active, RoleId = 1 },
                new Account { Id = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), UserName = "admin2", Email = "admin2@example.com", FullName = "Admin Two", PhoneNumber = "0987654321", PasswordHash = HashPassword("SecurePassword2!"), Status = AccountStatus.Active, RoleId = 1 },
                new Account { Id = Guid.Parse("7e006221-9a70-498d-a0b2-ae587c0cd1e8"), UserName = "manager1", Email = "manager1@example.com", FullName = "Manager One", PhoneNumber = "1122334455", PasswordHash = HashPassword("SecurePassword3!"), Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("4cab1ddc-9ebf-4488-aa28-c472393623ac"), UserName = "manager2", Email = "manager2@example.com", FullName = "Manager Two", PhoneNumber = "5566778899", PasswordHash = HashPassword("SecurePassword4!"), Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("88376119-6a82-489f-97d4-8b2ad19b7d67"), UserName = "manager3", Email = "manager3@example.com", FullName = "Manager Three", PhoneNumber = "6677889900", PasswordHash = HashPassword("SecurePassword5!"), Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), UserName = "staff1", Email = "staff1@example.com", FullName = "Staff One", PhoneNumber = "7788990011", PasswordHash = HashPassword("SecurePassword7!"), Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), UserName = "staff2", Email = "staff2@example.com", FullName = "Staff Two", PhoneNumber = "9900112233", PasswordHash = HashPassword("SecurePassword8!"), Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), UserName = "staff3", Email = "staff3@example.com", FullName = "Staff Three", PhoneNumber = "2233445566", PasswordHash = HashPassword("SecurePassword9!"), Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), UserName = "staff4", Email = "staff4@example.com", FullName = "Staff Four", PhoneNumber = "4455667788", PasswordHash = HashPassword("SecurePassword10!"), Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), UserName = "saleadmin1", Email = "saleadmin1@example.com", FullName = "Sale Admin 1", PhoneNumber = "938443122", PasswordHash = HashPassword("SecurePassword11!"), Status = AccountStatus.Active, RoleId = 4 },
                new Account { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), UserName = "ceo1", Email = "ceo1@example.com", FullName = "CEO 1", PhoneNumber = "5673434392", PasswordHash = HashPassword("SecurePassword12!"), Status = AccountStatus.Active, RoleId = 5 }
            );
            // 3. Seed Providers
            modelBuilder.Entity<Provider>().HasData(
                new Provider { ProviderId = 1, ProviderName = "ABC Pharma", Address = "12 Pharma St", PhoneNumber = "123456789", TaxCode = "TX123", Email = "abc@pharma.com", Status = ProviderStatus.Active, DocumentNumber = "GH12240001" },
                new Provider { ProviderId = 2, ProviderName = "XYZ Drugs", Address = "34 Medicine Rd", PhoneNumber = "987654321", TaxCode = "TX456", Email = "xyz@drugs.com", Status = ProviderStatus.Active, DocumentNumber = "GH12240002" },
                new Provider { ProviderId = 3, ProviderName = "MediCorp", Address = "56 Health Ave", PhoneNumber = "555555555", TaxCode = "TX789", Email = "contact@medicorp.com", Status = ProviderStatus.Active, DocumentNumber = "GH12240003" },
                new Provider { ProviderId = 4, ProviderName = "Wellness Inc", Address = "78 Wellness Ln", PhoneNumber = "111111111", TaxCode = "TX101", Email = "info@wellness.com", Status = ProviderStatus.Inactive, DocumentNumber = "GH12240004" },
                new Provider { ProviderId = 5, ProviderName = "SafeMeds", Address = "90 Secure Blvd", PhoneNumber = "222222222", TaxCode = "TX202", Email = "help@safemeds.com", Status = ProviderStatus.Active, DocumentNumber = "GH12240005" }
            );
            modelBuilder.Entity<Categories>().HasData(
                new Categories { CategoriesId = 1, CategoryName = "Prescription Drugs", ParentCategoryId = null, Description = "Medications that require a prescription from a doctor.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 2, CategoryName = "Over-the-Counter Drugs", ParentCategoryId = null, Description = "Medications that can be bought without a prescription.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 3, CategoryName = "Medical Devices", ParentCategoryId = null, Description = "Equipment used for medical purposes.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 4, CategoryName = "Skincare Products", ParentCategoryId = null, Description = "Cosmetic and therapeutic products for skincare.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 5, CategoryName = "Vitamins & Supplements", ParentCategoryId = null, Description = "Products that provide essential nutrients.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 6, CategoryName = "Pain Relievers", ParentCategoryId = 2, Description = "Medications to relieve pain.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 7, CategoryName = "Antibiotics", ParentCategoryId = 1, Description = "Drugs used to treat bacterial infections.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 8, CategoryName = "Diagnostic Tools", ParentCategoryId = 3, Description = "Devices used for medical diagnosis.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 9, CategoryName = "Eye Care Products", ParentCategoryId = null, Description = "Products for maintaining eye health.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 10, CategoryName = "Baby & Maternity Products", ParentCategoryId = null, Description = "Healthcare products for mothers and babies.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 11, CategoryName = "Cold & Flu Medicine", ParentCategoryId = 2, Description = "Medications for treating cold and flu symptoms.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 12, CategoryName = "First Aid Supplies", ParentCategoryId = 3, Description = "Basic medical supplies for first aid.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 13, CategoryName = "Anti-aging Products", ParentCategoryId = 4, Description = "Skincare products designed for anti-aging.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 14, CategoryName = "Probiotics", ParentCategoryId = 5, Description = "Supplements containing beneficial bacteria.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 15, CategoryName = "Topical Pain Relievers", ParentCategoryId = 6, Description = "Pain relief creams and gels.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 16, CategoryName = "Broad-Spectrum Antibiotics", ParentCategoryId = 7, Description = "Antibiotics effective against a wide range of bacteria.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 17, CategoryName = "Blood Pressure Monitors", ParentCategoryId = 8, Description = "Devices for monitoring blood pressure.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 18, CategoryName = "Contact Lens Solutions", ParentCategoryId = 9, Description = "Solutions for cleaning and storing contact lenses.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 19, CategoryName = "Prenatal Vitamins", ParentCategoryId = 10, Description = "Vitamins designed for pregnant women.", Status = CategoriesStatus.Active },
                new Categories { CategoriesId = 20, CategoryName = "Thermometers", ParentCategoryId = 8, Description = "Devices used to measure body temperature.", Status = CategoriesStatus.Active }
            );
            // 4. Seed Warehouses
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { WarehouseId = 1, WarehouseCode = "CW-1", WarehouseName = "Central Warehouse", Address = "123 Main St", Status = WarehouseStatus.Active },
                new Warehouse { WarehouseId = 2, WarehouseCode = "EW-1", WarehouseName = "East Warehouse", Address = "456 East St", Status = WarehouseStatus.Active },
                new Warehouse { WarehouseId = 3, WarehouseCode = "WW-1", WarehouseName = "West Warehouse", Address = "789 West St", Status = WarehouseStatus.Active },
                new Warehouse { WarehouseId = 4, WarehouseCode = "NW-1", WarehouseName = "North Warehouse", Address = "321 North St", Status = WarehouseStatus.Active },
                new Warehouse { WarehouseId = 5, WarehouseCode = "SW-1", WarehouseName = "South Warehouse", Address = "654 South St", Status = WarehouseStatus.Active }
            );
            // 5. Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, ProductName = "Paracetamol", ProductCode = "P001", SKU = "Tablet", MadeFrom = "Chemical" },
                new Product { ProductId = 2, ProductName = "Aspirin", ProductCode = "P002", SKU = "Capsule", MadeFrom = "Chemical" },
                new Product { ProductId = 3, ProductName = "Ibuprofen", ProductCode = "P003", SKU = "Gel", MadeFrom = "Chemical" },
                new Product { ProductId = 4, ProductName = "Vitamin C", ProductCode = "P004", SKU = "Syrup", MadeFrom = "Natural" },
                new Product { ProductId = 5, ProductName = "Cough Syrup", ProductCode = "P005", SKU = "Liquid", MadeFrom = "Herbal" },
                new Product { ProductId = 6, ProductName = "Amoxicillin", ProductCode = "P006", SKU = "Antibiotic", MadeFrom = "Chemical" },
                new Product { ProductId = 7, ProductName = "Cetirizine", ProductCode = "P007", SKU = "Tablet", MadeFrom = "Chemical" },
                new Product { ProductId = 8, ProductName = "Probiotic A", ProductCode = "P008", SKU = "Capsule", MadeFrom = "Natural" },
                new Product { ProductId = 9, ProductName = "Skincare B", ProductCode = "P009", SKU = "Cream", MadeFrom = "Natural" },
                new Product { ProductId = 10, ProductName = "Herbal Tea X", ProductCode = "P010", SKU = "Tea", MadeFrom = "Herbal" }
    );
            // 6. Seed Lots (10 dòng)
            // modelBuilder.Entity<Lot>().HasData(
            //     new Lot { LotId = 1, LotNumber = "L001", Quantity = 100, ManufacturingDate = new DateOnly(2023, 1, 1), ExpiryDate = new DateOnly(2025, 12, 31), WarehouseId = 1, ProductId = 1, ProviderId = 1 },
            //     new Lot { LotId = 2, LotNumber = "L002", Quantity = 200, ManufacturingDate = new DateOnly(2023, 3, 15), ExpiryDate = new DateOnly(2026, 10, 20), WarehouseId = 2, ProductId = 2, ProviderId = 2 },
            //     new Lot { LotId = 3, LotNumber = "L003", Quantity = 150, ManufacturingDate = new DateOnly(2023, 5, 10), ExpiryDate = new DateOnly(2026, 3, 25), WarehouseId = 3, ProductId = 3, ProviderId = 3 },
            //     new Lot { LotId = 4, LotNumber = "L004", Quantity = 250, ManufacturingDate = new DateOnly(2022, 12, 5), ExpiryDate = new DateOnly(2025, 11, 30), WarehouseId = 4, ProductId = 4, ProviderId = 4 },
            //     new Lot { LotId = 5, LotNumber = "L005", Quantity = 300, ManufacturingDate = new DateOnly(2024, 2, 20), ExpiryDate = new DateOnly(2026, 9, 15), WarehouseId = 5, ProductId = 5, ProviderId = 5 },
            //     new Lot { LotId = 6, LotNumber = "L006", Quantity = 50, ManufacturingDate = new DateOnly(2024, 3, 1), ExpiryDate = new DateOnly(2025, 12, 31), WarehouseId = 1, ProductId = 6, ProviderId = 1 },
            //     new Lot { LotId = 7, LotNumber = "L007", Quantity = 8, ManufacturingDate = new DateOnly(2023, 2, 10), ExpiryDate = new DateOnly(2024, 8, 10), WarehouseId = 2, ProductId = 7, ProviderId = 2 },
            //     new Lot { LotId = 8, LotNumber = "L008", Quantity = 95, ManufacturingDate = new DateOnly(2023, 7, 1), ExpiryDate = new DateOnly(2025, 7, 1), WarehouseId = 3, ProductId = 8, ProviderId = 3 },
            //     new Lot { LotId = 9, LotNumber = "L009", Quantity = 4, ManufacturingDate = new DateOnly(2024, 1, 20), ExpiryDate = new DateOnly(2025, 2, 15), WarehouseId = 4, ProductId = 9, ProviderId = 4 },
            //     new Lot { LotId = 10, LotNumber = "L010", Quantity = 12, ManufacturingDate = new DateOnly(2024, 5, 1), ExpiryDate = new DateOnly(2025, 5, 15), WarehouseId = 5, ProductId = 10, ProviderId = 5 },
            //     new Lot { LotId = 11, LotNumber = "L011", Quantity = 60, ManufacturingDate = new DateOnly(2024, 1, 1), ExpiryDate = new DateOnly(2026, 1, 1), WarehouseId = 1, ProductId = 1, ProviderId = 1 },
            //     new Lot { LotId = 12, LotNumber = "L012", Quantity = 40, ManufacturingDate = new DateOnly(2024, 2, 1), ExpiryDate = new DateOnly(2026, 2, 1), WarehouseId = 1, ProductId = 1, ProviderId = 1 }
            //     );

            
            modelBuilder.Entity<Lot>().HasData(
                new Lot { LotId = 1, LotNumber = "LOT-001", ProductId = 1, ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 03, 01)), Quantity = 500, ProviderId = 1, WarehouseId = 1 },
                new Lot { LotId = 2, LotNumber = "LOT-002", ProductId = 2, ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 03, 05)), Quantity = 300, ProviderId = 2, WarehouseId = 2 },
                new Lot { LotId = 3, LotNumber = "LOT-003", ProductId = 3, ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 03, 10)), Quantity = 200, ProviderId = 3, WarehouseId = 3 },
                new Lot { LotId = 4, LotNumber = "LOT-004", ProductId = 4, ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 03, 15)), Quantity = 400, ProviderId = 4, WarehouseId = 4 },
                new Lot { LotId = 5, LotNumber = "LOT-005", ProductId = 5, ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 03, 20)), Quantity = 250, ProviderId = 5, WarehouseId = 5 }
            );

            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, CustomerName = "John Doe", Address = "123 Main St", PhoneNumber = "555-1234", Email = "john.doe@example.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "GH12240001" },
                new Customer { CustomerId = 2, CustomerName = "Jane Smith", Address = "456 Elm St", PhoneNumber = "555-5678", Email = "jane.smith@example.com", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "GH12240002" },
                new Customer { CustomerId = 3, CustomerName = "Alice Johnson", Address = "789 Oak St", PhoneNumber = "555-8765", Email = "alice.johnson@example.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "GH12240003" },
                new Customer { CustomerId = 4, CustomerName = "Bob Brown", Address = "321 Pine St", PhoneNumber = "555-4321", Email = "bob.brown@example.com", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "GH12240004" },
                new Customer { CustomerId = 5, CustomerName = "Charlie Davis", Address = "654 Maple St", PhoneNumber = "555-6789", Email = "charlie.davis@example.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "GH12240005" },
                new Customer { CustomerId = 6, CustomerName = "Diana Evans", Address = "987 Birch St", PhoneNumber = "555-9876", Email = "diana.evans@example.com", IsLoyal = false, Status = CustomerStatus.Inactive, DocumentNumber = "GH12240006" },
                new Customer { CustomerId = 7, CustomerName = "Eve Foster", Address = "159 Cedar St", PhoneNumber = "555-1597", Email = "eve.foster@example.com", IsLoyal = true, Status = CustomerStatus.Inactive, DocumentNumber = "GH12240007" },
                new Customer { CustomerId = 8, CustomerName = "Frank Green", Address = "753 Spruce St", PhoneNumber = "555-7531", Email = "frank.green@example.com", IsLoyal = false, Status = CustomerStatus.Inactive, DocumentNumber = "GH12240008" },
                new Customer { CustomerId = 9, CustomerName = "Grace Harris", Address = "852 Willow St", PhoneNumber = "555-8524", Email = "grace.harris@example.com", IsLoyal = true, Status = CustomerStatus.Inactive, DocumentNumber = "GH12240009" },
                new Customer { CustomerId = 10, CustomerName = "Henry Irving", Address = "951 Poplar St", PhoneNumber = "555-9513", Email = "henry.irving@example.com", IsLoyal = false, Status = CustomerStatus.Inactive, DocumentNumber = "GH122400010" }
            );

            // ================== Outbound (10 dòng) ==================
            modelBuilder.Entity<Outbound>().HasData(
                new Outbound { OutboundId = 1, OutboundCode = "OB001", OutboundOrderCode = "ORD001", OutboundDate = instantNow, Status = OutboundStatus.Completed, AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), CustomerId = 1 },
                new Outbound { OutboundId = 2, OutboundCode = "OB002", OutboundOrderCode = "ORD002", OutboundDate = instantNow, Status = OutboundStatus.Completed, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CustomerId = 2 },
                new Outbound { OutboundId = 3, OutboundCode = "OB003", OutboundOrderCode = "ORD003", OutboundDate = instantNow, Status = OutboundStatus.Completed, AccountId = Guid.Parse("7e006221-9a70-498d-a0b2-ae587c0cd1e8"), CustomerId = 3 },
                new Outbound { OutboundId = 4, OutboundCode = "OB004", OutboundOrderCode = "ORD004", OutboundDate = instantNow, Status = OutboundStatus.Completed, AccountId = Guid.Parse("4cab1ddc-9ebf-4488-aa28-c472393623ac"), CustomerId = 4 },
                new Outbound { OutboundId = 5, OutboundCode = "OB005", OutboundOrderCode = "ORD005", OutboundDate = instantNow, Status = OutboundStatus.Completed, AccountId = Guid.Parse("88376119-6a82-489f-97d4-8b2ad19b7d67"), CustomerId = 5 },
                new Outbound { OutboundId = 6, OutboundCode = "OB006", OutboundOrderCode = "ORD006", OutboundDate = instantNow.Plus(Duration.FromDays(10)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("7e006221-9a70-498d-a0b2-ae587c0cd1e8"), CustomerId = 6 },
                new Outbound { OutboundId = 7, OutboundCode = "OB007", OutboundOrderCode = "ORD007", OutboundDate = instantNow.Plus(Duration.FromDays(15)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), CustomerId = 7 },
                new Outbound { OutboundId = 8, OutboundCode = "OB008", OutboundOrderCode = "ORD008", OutboundDate = instantNow.Plus(Duration.FromDays(20)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CustomerId = 8 },
                new Outbound { OutboundId = 9, OutboundCode = "OB009", OutboundOrderCode = "ORD009", OutboundDate = instantNow.Plus(Duration.FromDays(25)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("4cab1ddc-9ebf-4488-aa28-c472393623ac"), CustomerId = 9 },
                new Outbound { OutboundId = 10, OutboundCode = "OB010", OutboundOrderCode = "ORD010", OutboundDate = instantNow.Plus(Duration.FromDays(30)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("88376119-6a82-489f-97d4-8b2ad19b7d67"), CustomerId = 10 }
            );

            // ================== OutboundDetails (10 dòng) ==================
            modelBuilder.Entity<OutboundDetails>().HasData(
                new OutboundDetails { OutboundDetailsId = 1, LotNumber = "L001", ExpiryDate = new DateOnly(2025, 12, 31), Quantity = 50, UnitType = "Box", UnitPrice = 12.5m, TotalPrice = 625m, OutboundId = 1, LotId = 1 },
                new OutboundDetails { OutboundDetailsId = 2, LotNumber = "L002", ExpiryDate = new DateOnly(2026, 10, 20), Quantity = 30, UnitType = "Bottle", UnitPrice = 20m, TotalPrice = 600m, OutboundId = 2, LotId = 2 },
                new OutboundDetails { OutboundDetailsId = 3, LotNumber = "L003", ExpiryDate = new DateOnly(2026, 3, 25), Quantity = 40, UnitType = "Pack", UnitPrice = 8m, TotalPrice = 320m, OutboundId = 3, LotId = 3 },
                new OutboundDetails { OutboundDetailsId = 4, LotNumber = "L004", ExpiryDate = new DateOnly(2025, 11, 30), Quantity = 20, UnitType = "Vial", UnitPrice = 50m, TotalPrice = 1000m, OutboundId = 4, LotId = 4 },
                new OutboundDetails { OutboundDetailsId = 5, LotNumber = "L005", ExpiryDate = new DateOnly(2026, 9, 15), Quantity = 25, UnitType = "Tube", UnitPrice = 15m, TotalPrice = 375m, OutboundId = 5, LotId = 5 }
                // new OutboundDetails { OutboundDetailsId = 6, LotNumber = "L006", ExpiryDate = new DateOnly(2025, 12, 31), Quantity = 15, UnitType = "Box", UnitPrice = 10m, TotalPrice = 150m, OutboundId = 6, LotId = 6 },
                // new OutboundDetails { OutboundDetailsId = 7, LotNumber = "L007", ExpiryDate = new DateOnly(2024, 8, 10), Quantity = 8, UnitType = "Bottle", UnitPrice = 9m, TotalPrice = 72m, OutboundId = 7, LotId = 7 }, // điều chỉnh 10 -> 8
                // new OutboundDetails { OutboundDetailsId = 8, LotNumber = "L008", ExpiryDate = new DateOnly(2025, 7, 1), Quantity = 5, UnitType = "Pack", UnitPrice = 8.5m, TotalPrice = 42.5m, OutboundId = 8, LotId = 8 },
                // new OutboundDetails { OutboundDetailsId = 9, LotNumber = "L009", ExpiryDate = new DateOnly(2025, 2, 15), Quantity = 4, UnitType = "Vial", UnitPrice = 7m, TotalPrice = 28m, OutboundId = 9, LotId = 9 },  // điều chỉnh 28 -> 4
                // new OutboundDetails { OutboundDetailsId = 10, LotNumber = "L010", ExpiryDate = new DateOnly(2025, 5, 15), Quantity = 12, UnitType = "Tube", UnitPrice = 6.5m, TotalPrice = 78m, OutboundId = 10, LotId = 10 } // điều chỉnh 33 -> 12
            );
            // ================== LotTransfer (10 dòng) ==================
            modelBuilder.Entity<LotTransfer>().HasData(
                new LotTransfer { LotTransferId = 1, LotTransferCode = "LT-001", LotTransferStatus = LotTransferStatus.Completed, FromWareHouseId = 1, ToWareHouseId = 2, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 01))) },
                new LotTransfer { LotTransferId = 2, LotTransferCode = "LT-002", LotTransferStatus = LotTransferStatus.Completed, FromWareHouseId = 2, ToWareHouseId = 3, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 05))) },
                new LotTransfer { LotTransferId = 3, LotTransferCode = "LT-003", LotTransferStatus = LotTransferStatus.Completed, FromWareHouseId = 1, ToWareHouseId = 3, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 10))) },
                new LotTransfer { LotTransferId = 4, LotTransferCode = "LT-004", LotTransferStatus = LotTransferStatus.Completed, FromWareHouseId = 3, ToWareHouseId = 4, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 15))) },
                new LotTransfer { LotTransferId = 5, LotTransferCode = "LT-005", LotTransferStatus = LotTransferStatus.Completed, FromWareHouseId = 2, ToWareHouseId = 1, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 20))) }
            );
            // ================== LotTransferDetail (10 dòng) ==================
            modelBuilder.Entity<LotTransferDetail>().HasData(
                new LotTransferDetail { LotTransferDetailId = 1, LotTransferId = 1, LotId = 1, Quantity = 50 },
                new LotTransferDetail { LotTransferDetailId = 2, LotTransferId = 1, LotId = 2, Quantity = 30 },
                new LotTransferDetail { LotTransferDetailId = 3, LotTransferId = 2, LotId = 3, Quantity = 20 },
                new LotTransferDetail { LotTransferDetailId = 4, LotTransferId = 3, LotId = 4, Quantity = 40 },
                new LotTransferDetail { LotTransferDetailId = 5, LotTransferId = 4, LotId = 5, Quantity = 25 }
            );
            modelBuilder.Entity<InboundRequest>().HasData(
                new InboundRequest { InboundRequestId = 1, InboundRequestCode = "REQ-001", AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 01))), Status = InboundRequestStatus.Completed },
                new InboundRequest { InboundRequestId = 2, InboundRequestCode = "REQ-002", AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 05))), Status = InboundRequestStatus.Completed },
                new InboundRequest { InboundRequestId = 3, InboundRequestCode = "REQ-003", AccountId = Guid.Parse("4cab1ddc-9ebf-4488-aa28-c472393623ac"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 10))), Status = InboundRequestStatus.Completed },
                new InboundRequest { InboundRequestId = 4, InboundRequestCode = "REQ-004", AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 15))), Status = InboundRequestStatus.Completed },
                new InboundRequest { InboundRequestId = 5, InboundRequestCode = "REQ-005", AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 20))), Status = InboundRequestStatus.Completed }
            );

            modelBuilder.Entity<InboundRequestDetails>().HasData(
                new InboundRequestDetails { InboundRequestDetailsId = 1, InboundRequestId = 1, ProductId = 1, Quantity = 500 },
                new InboundRequestDetails { InboundRequestDetailsId = 2, InboundRequestId = 2, ProductId = 2, Quantity = 300 },
                new InboundRequestDetails { InboundRequestDetailsId = 3, InboundRequestId = 3, ProductId = 3, Quantity = 200 },
                new InboundRequestDetails { InboundRequestDetailsId = 4, InboundRequestId = 4, ProductId = 4, Quantity = 400 },
                new InboundRequestDetails { InboundRequestDetailsId = 5, InboundRequestId = 5, ProductId = 5, Quantity = 250 }
            );
            modelBuilder.Entity<Inbound>().HasData(
                new Inbound { InboundId = 1, InboundCode = "INB-001", InboundRequestId = 1, WarehouseId = 1, ProviderId = 1, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 02))), Status = InboundStatus.Completed },
                new Inbound { InboundId = 2, InboundCode = "INB-002", InboundRequestId = 2, WarehouseId = 2, ProviderId = 2, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 06))), Status = InboundStatus.Completed },
                new Inbound { InboundId = 3, InboundCode = "INB-003", InboundRequestId = 3, WarehouseId = 1, ProviderId = 3, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 11))), Status = InboundStatus.Completed },
                new Inbound { InboundId = 4, InboundCode = "INB-004", InboundRequestId = 4, WarehouseId = 3, ProviderId = 4, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 16))), Status = InboundStatus.Completed },
                new Inbound { InboundId = 5, InboundCode = "INB-005", InboundRequestId = 5, WarehouseId = 2, ProviderId = 5, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), CreatedAt = Instant.FromDateTimeOffset(new DateTimeOffset(new DateTime(2025, 03, 21))), Status = InboundStatus.Completed }
            );

            modelBuilder.Entity<InboundDetails>().HasData(
                new InboundDetails { InboundDetailsId = 1, InboundId = 1, ProductId = 1, Quantity = 500, LotNumber = "LOT-001" },
                new InboundDetails { InboundDetailsId = 2, InboundId = 1, ProductId = 2, Quantity = 300, LotNumber = "LOT-002" },
                new InboundDetails { InboundDetailsId = 3, InboundId = 2, ProductId = 3, Quantity = 200, LotNumber = "LOT-003" },
                new InboundDetails { InboundDetailsId = 4, InboundId = 3, ProductId = 4, Quantity = 400, LotNumber = "LOT-004" },
                new InboundDetails { InboundDetailsId = 5, InboundId = 4, ProductId = 5, Quantity = 250, LotNumber = "LOT-005" }
            );

        }
        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }
    }
}
