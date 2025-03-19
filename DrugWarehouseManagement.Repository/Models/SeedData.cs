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
                new Warehouse { WarehouseId = 2, WarehouseCode = "EW-1", WarehouseName = "East Warehouse", Address = "456 East St", Status = WarehouseStatus.Inactive },
                new Warehouse { WarehouseId = 3, WarehouseCode = "WW-1", WarehouseName = "West Warehouse", Address = "789 West St", Status = WarehouseStatus.Inactive },
                new Warehouse { WarehouseId = 4, WarehouseCode = "NW-1", WarehouseName = "North Warehouse", Address = "321 North St", Status = WarehouseStatus.Active },
                new Warehouse { WarehouseId = 5, WarehouseCode = "SW-1", WarehouseName = "South Warehouse", Address = "654 South St", Status = WarehouseStatus.Active }
            );
            // 5. Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, ProductName = "Paracetamol", ProductCode = "P001", SKU = "Tablet", MadeFrom = "Chemical" },
                new Product { ProductId = 2, ProductName = "Aspirin", ProductCode = "P002", SKU = "Capsule", MadeFrom = "Chemical" },
                new Product { ProductId = 3, ProductName = "Ibuprofen", ProductCode = "P003", SKU = "Gel", MadeFrom = "Chemical" },
                new Product { ProductId = 4, ProductName = "Vitamin C", ProductCode = "P004", SKU = "Syrup", MadeFrom = "Natural" },
                new Product { ProductId = 5, ProductName = "Cough Syrup", ProductCode = "P005", SKU = "Liquid", MadeFrom = "Herbal" }
            );
            // 6. Seed Lots
            modelBuilder.Entity<Lot>().HasData(
                 new Lot { LotId = 1, LotNumber = "L001", Quantity = 100, ManufacturingDate = new DateOnly(2024, 1, 1), ExpiryDate = new DateOnly(2026, 12, 31), WarehouseId = 1, ProductId = 1, ProviderId = 1 },
                 new Lot { LotId = 2, LotNumber = "L002", Quantity = 200, ManufacturingDate = new DateOnly(2023, 6, 15), ExpiryDate = new DateOnly(2026, 10, 20), WarehouseId = 2, ProductId = 2, ProviderId = 2 },
                 new Lot { LotId = 3, LotNumber = "L003", Quantity = 150, ManufacturingDate = new DateOnly(2023, 8, 10), ExpiryDate = new DateOnly(2027, 3, 25), WarehouseId = 3, ProductId = 3, ProviderId = 3 },
                 new Lot { LotId = 4, LotNumber = "L004", Quantity = 250, ManufacturingDate = new DateOnly(2022, 12, 5), ExpiryDate = new DateOnly(2025, 11, 30), WarehouseId = 4, ProductId = 4, ProviderId = 4 },
                 new Lot { LotId = 5, LotNumber = "L005", Quantity = 300, ManufacturingDate = new DateOnly(2024, 2, 20), ExpiryDate = new DateOnly(2026, 9, 15), WarehouseId = 5, ProductId = 5, ProviderId = 5 },

                 // New Lots with quantity < 10 and expiry within 1 year or < 60% shelf life
                 new Lot { LotId = 11, LotNumber = "L011", Quantity = 5, ManufacturingDate = new DateOnly(2023, 3, 10), ExpiryDate = new DateOnly(2025, 4, 1), WarehouseId = 2, ProductId = 2, ProviderId = 2 },
                 new Lot { LotId = 12, LotNumber = "L012", Quantity = 7, ManufacturingDate = new DateOnly(2023, 8, 5), ExpiryDate = new DateOnly(2025, 6, 30), WarehouseId = 3, ProductId = 3, ProviderId = 3 },
                 new Lot { LotId = 13, LotNumber = "L013", Quantity = 3, ManufacturingDate = new DateOnly(2024, 1, 20), ExpiryDate = new DateOnly(2025, 2, 15), WarehouseId = 1, ProductId = 4, ProviderId = 4 },
                 new Lot { LotId = 14, LotNumber = "L014", Quantity = 9, ManufacturingDate = new DateOnly(2024, 5, 1), ExpiryDate = new DateOnly(2025, 5, 15), WarehouseId = 4, ProductId = 5, ProviderId = 5 }
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

            // 7. Seed Outbounds
            modelBuilder.Entity<Outbound>().HasData(
                new Outbound { OutboundId = 1, OutboundCode = "OB001", OutboundOrderCode = "ORD001", OutboundDate = instantNow, Status = OutboundStatus.Pending, AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), CustomerId = 1 },
                new Outbound { OutboundId = 2, OutboundCode = "OB002", OutboundOrderCode = "ORD002", OutboundDate = instantNow, Status = OutboundStatus.InProgress, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CustomerId = 2 },
                new Outbound { OutboundId = 3, OutboundCode = "OB003", OutboundOrderCode = "ORD003", OutboundDate = instantNow, Status = OutboundStatus.Completed, AccountId = Guid.Parse("7e006221-9a70-498d-a0b2-ae587c0cd1e8"), CustomerId = 3 },
                new Outbound { OutboundId = 4, OutboundCode = "OB004", OutboundOrderCode = "ORD004", OutboundDate = instantNow, Status = OutboundStatus.Completed, AccountId = Guid.Parse("4cab1ddc-9ebf-4488-aa28-c472393623ac"), CustomerId = 4 },
                new Outbound { OutboundId = 5, OutboundCode = "OB005", OutboundOrderCode = "ORD005", OutboundDate = instantNow, Status = OutboundStatus.Cancelled, AccountId = Guid.Parse("88376119-6a82-489f-97d4-8b2ad19b7d67"), CustomerId = 5 }
            );

            // 9. Seed OutboundDetails
            modelBuilder.Entity<OutboundDetails>().HasData(
                new OutboundDetails { OutboundDetailsId = 1, LotNumber = "LOT-20240101", ExpiryDate = new DateOnly(2026, 12, 31), Quantity = 50, UnitType = "Box", UnitPrice = 12.5m, TotalPrice = 625m, OutboundId = 1, LotId = 1 },
                new OutboundDetails { OutboundDetailsId = 2, LotNumber = "LOT-20240201", ExpiryDate = new DateOnly(2026, 12, 31), Quantity = 30, UnitType = "Bottle", UnitPrice = 20m, TotalPrice = 600m, OutboundId = 2, LotId = 2 },
                new OutboundDetails { OutboundDetailsId = 3, LotNumber = "LOT-20240301", ExpiryDate = new DateOnly(2026, 12, 31), Quantity = 40, UnitType = "Pack", UnitPrice = 8m, TotalPrice = 320m, OutboundId = 3, LotId = 3 },
                new OutboundDetails { OutboundDetailsId = 4, LotNumber = "LOT-20240401", ExpiryDate = new DateOnly(2026, 12, 31), Quantity = 20, UnitType = "Vial", UnitPrice = 50m, TotalPrice = 1000m, OutboundId = 4, LotId = 4 },
                new OutboundDetails { OutboundDetailsId = 5, LotNumber = "LOT-20240501", ExpiryDate = new DateOnly(2026, 12, 31), Quantity = 25, UnitType = "Tube", UnitPrice = 15m, TotalPrice = 375m, OutboundId = 5, LotId = 5 }
            );
            //10. Seed LotTransfers
            modelBuilder.Entity<LotTransfer>().HasData(
                new LotTransfer { LotTransferId = 1, LotTransferCode = "TO-1001", LotTransferStatus = LotTransferStatus.Pending, FromWareHouseId = 1, ToWareHouseId = 2, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 2, LotTransferCode = "TO-1002", LotTransferStatus = LotTransferStatus.Completed, FromWareHouseId = 2, ToWareHouseId = 3, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 3, LotTransferCode = "TO-1003", LotTransferStatus = LotTransferStatus.Pending, FromWareHouseId = 3, ToWareHouseId = 4, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 4, LotTransferCode = "TO-1004", LotTransferStatus = LotTransferStatus.InProgress, FromWareHouseId = 4, ToWareHouseId = 5, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 5, LotTransferCode = "TO-1005", LotTransferStatus = LotTransferStatus.Cancelled, FromWareHouseId = 5, ToWareHouseId = 1, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 6, LotTransferCode = "TO-1006", LotTransferStatus = LotTransferStatus.Completed, FromWareHouseId = 1, ToWareHouseId = 3, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 7, LotTransferCode = "TO-1007", LotTransferStatus = LotTransferStatus.Pending, FromWareHouseId = 2, ToWareHouseId = 4, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 8, LotTransferCode = "TO-1008", LotTransferStatus = LotTransferStatus.InProgress, FromWareHouseId = 3, ToWareHouseId = 5, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 9, LotTransferCode = "TO-1009", LotTransferStatus = LotTransferStatus.Completed, FromWareHouseId = 4, ToWareHouseId = 1, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 10, LotTransferCode = "TO-1010", LotTransferStatus = LotTransferStatus.Pending, FromWareHouseId = 5, ToWareHouseId = 2, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new LotTransfer { LotTransferId = 11, LotTransferCode = "TO-1011", LotTransferStatus = LotTransferStatus.Completed, FromWareHouseId = 1, ToWareHouseId = 2, AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), CreatedAt = instantNow.Plus(Duration.FromDays(45)) }
                );
            //11. Seed LotTransferDetails
            modelBuilder.Entity<LotTransferDetail>().HasData(
                new LotTransferDetail { LotTransferDetailId = 1, Quantity = 10, ProductId = 1, LotId = 1, LotTransferId = 1, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 2, Quantity = 15, ProductId = 2, LotId = 2, LotTransferId = 1, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 3, Quantity = 5, ProductId = 3, LotId = 3, LotTransferId = 2, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 4, Quantity = 20, ProductId = 4, LotId = 4, LotTransferId = 3, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 5, Quantity = 12, ProductId = 5, LotId = 5, LotTransferId = 3, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 6, Quantity = 25, ProductId = 1, LotId = 5, LotTransferId = 4, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 7, Quantity = 30, ProductId = 2, LotId = 4, LotTransferId = 5, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 8, Quantity = 8, ProductId = 3, LotId = 3, LotTransferId = 6, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 9, Quantity = 14, ProductId = 4, LotId = 2, LotTransferId = 7, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 10, Quantity = 18, ProductId = 5, LotId = 1, LotTransferId = 8, ExpiryDate = new DateOnly(2026, 12, 31) },
                new LotTransferDetail { LotTransferDetailId = 11, Quantity = 10, ProductId = 1, LotId = 1, LotTransferId = 11, ExpiryDate = new DateOnly(2026, 12, 31) }

            );
            //12. Seed Inbounds
            modelBuilder.Entity<Inbound>().HasData(
                new Inbound { InboundId = 1, InboundCode = "IB001", ProviderOrderCode = "PORD001", Note = "First inbound - pending", InboundDate = instantNow, Status = InboundStatus.Pending, ProviderId = 1, AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), WarehouseId = 1 },
                new Inbound { InboundId = 2, InboundCode = "IB002", ProviderOrderCode = "PORD002", Note = "Second inbound - completed", InboundDate = instantNow, Status = InboundStatus.Completed, ProviderId = 2, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), WarehouseId = 2 },
                new Inbound { InboundId = 3, InboundCode = "IB003", ProviderOrderCode = "PORD003", Note = "Third inbound - cancelled", InboundDate = instantNow, Status = InboundStatus.Cancelled, ProviderId = 3, AccountId = Guid.Parse("7e006221-9a70-498d-a0b2-ae587c0cd1e8"), WarehouseId = 3 },
                new Inbound { InboundId = 4, InboundCode = "IB004", ProviderOrderCode = "PORD004", Note = "Fourth inbound - pending", InboundDate = instantNow, Status = InboundStatus.Pending, ProviderId = 4, AccountId = Guid.Parse("4cab1ddc-9ebf-4488-aa28-c472393623ac"), WarehouseId = 4 },
                new Inbound { InboundId = 5, InboundCode = "IB005", ProviderOrderCode = "PORD005",  Note = "Fifth inbound - completed", InboundDate = instantNow, Status = InboundStatus.Completed, ProviderId = 5, AccountId = Guid.Parse("88376119-6a82-489f-97d4-8b2ad19b7d67"), WarehouseId = 5 },
                new Inbound { InboundId = 6, InboundCode = "IB006", ProviderOrderCode = "PORD006",  Note = "Inbound for opening stock", InboundDate = instantNow.Minus(Duration.FromDays(400)), Status = InboundStatus.Completed, ProviderId = 1, AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"),  WarehouseId = 1 },
                new Inbound { InboundId = 7, InboundCode = "IB007", ProviderOrderCode = "PORD007", Note = "Inbound within date range", InboundDate = instantNow.Plus(Duration.FromDays(40)), Status = InboundStatus.Completed, ProviderId = 2, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), WarehouseId = 1 }
            );
            //13. Seed InboundDetails
            modelBuilder.Entity<InboundDetails>().HasData(
                new InboundDetails { InboundDetailsId = 1, LotNumber = "LOT-INB001-A", ManufacturingDate = new DateOnly(2023, 1, 1), ExpiryDate = new DateOnly(2025, 12, 31), Quantity = 50, UnitPrice = 10m, OpeningStock = 20, TotalPrice = 500m, InboundId = 1 },
                new InboundDetails { InboundDetailsId = 2, LotNumber = "LOT-INB002-A", ManufacturingDate = new DateOnly(2022, 6, 1), ExpiryDate = new DateOnly(2024, 12, 31), Quantity = 80, UnitPrice = 12.5m, OpeningStock = 30, TotalPrice = 1000m, InboundId = 2 },
                new InboundDetails { InboundDetailsId = 3, LotNumber = "LOT-INB003-A", ManufacturingDate = new DateOnly(2023, 5, 10), ExpiryDate = new DateOnly(2026, 3, 15), Quantity = 100, UnitPrice = 5m, OpeningStock = 10, TotalPrice = 500m, InboundId = 3 },
                new InboundDetails { InboundDetailsId = 4, LotNumber = "LOT-INB004-A", ManufacturingDate = new DateOnly(2022, 10, 25), ExpiryDate = new DateOnly(2025, 9, 1), Quantity = 120, UnitPrice = 7m, OpeningStock = 25, TotalPrice = 840m, InboundId = 4 },
                new InboundDetails { InboundDetailsId = 5, LotNumber = "LOT-INB005-A", ManufacturingDate = new DateOnly(2023, 2, 5), ExpiryDate = new DateOnly(2026, 6, 20), Quantity = 60, UnitPrice = 15m, OpeningStock = 15, TotalPrice = 900m, InboundId = 5 },
                new InboundDetails { InboundDetailsId = 6, LotNumber = "LOT-INB006-A", ManufacturingDate = new DateOnly(2024, 6, 1), ExpiryDate = new DateOnly(2026, 12, 31), Quantity = 40, UnitPrice = 2m, OpeningStock = 10, TotalPrice = 80m, InboundId = 6 },
                new InboundDetails { InboundDetailsId = 7, LotNumber = "LOT-INB007-A", ManufacturingDate = new DateOnly(2024, 7, 1), ExpiryDate = new DateOnly(2026, 12, 31), Quantity = 60, UnitPrice = 2m, OpeningStock = 5, TotalPrice = 120m, InboundId = 7 }
            );
        }
        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }
    }
}
