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
                new Warehouse { WarehouseId = 1, WarehouseCode = "CW-1", WarehouseName = "Central Warehouse", Address = "123 Main St", Status = WarehouseStatus.Active , DocumentNumber = "WH12346"},
                new Warehouse { WarehouseId = 2, WarehouseCode = "EW-1", WarehouseName = "East Warehouse", Address = "456 East St", Status = WarehouseStatus.Active, DocumentNumber ="WH654321" },
                new Warehouse { WarehouseId = 3, WarehouseCode = "WW-1", WarehouseName = "West Warehouse", Address = "789 West St", Status = WarehouseStatus.Active, DocumentNumber = "WH11234" },
                new Warehouse { WarehouseId = 4, WarehouseCode = "NW-1", WarehouseName = "North Warehouse", Address = "321 North St", Status = WarehouseStatus.Active,DocumentNumber = "WH12346" },
                new Warehouse { WarehouseId = 5, WarehouseCode = "SW-1", WarehouseName = "South Warehouse", Address = "654 South St", Status = WarehouseStatus.Active, DocumentNumber = "WH123367"}
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


            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, CustomerName = "John Doe", Address = "123 Main St", PhoneNumber = "555-1234", Email = "john.doe@example.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "GH12240001" },
                new Customer { CustomerId = 2, CustomerName = "Jane Smith", Address = "456 Elm St", PhoneNumber = "555-5678", Email = "jane.smith@example.com", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "GH12240002" },
                new Customer { CustomerId = 3, CustomerName = "Alice Johnson", Address = "789 Oak St", PhoneNumber = "555-8765", Email = "alice.johnson@example.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "GH12240003" },
                new Customer { CustomerId = 4, CustomerName = "Bob Brown", Address = "321 Pine St", PhoneNumber = "555-4321", Email = "bob.brown@example.com", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "GH12240004" },
                new Customer { CustomerId = 5, CustomerName = "Charlie Davis", Address = "654 Maple St", PhoneNumber = "555-6789", Email = "charlie.davis@example.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "GH12240005" },
                new Customer { CustomerId = 6, CustomerName = "Diana Evans", Address = "987 Birch St", PhoneNumber = "555-9876", Email = "diana.evans@example.com", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "GH12240006" },
                new Customer { CustomerId = 7, CustomerName = "Eve Foster", Address = "159 Cedar St", PhoneNumber = "555-1597", Email = "eve.foster@example.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "GH12240007" },
                new Customer { CustomerId = 8, CustomerName = "Frank Green", Address = "753 Spruce St", PhoneNumber = "555-7531", Email = "frank.green@example.com", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "GH12240008" },
                new Customer { CustomerId = 9, CustomerName = "Grace Harris", Address = "852 Willow St", PhoneNumber = "555-8524", Email = "grace.harris@example.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "GH12240009" },
                new Customer { CustomerId = 10, CustomerName = "Henry Irving", Address = "951 Poplar St", PhoneNumber = "555-9513", Email = "henry.irving@example.com", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "GH122400010" }
            );



            modelBuilder.Entity<InboundRequest>().HasData(
                     new InboundRequest { InboundRequestId = 1, InboundRequestCode = "REQ-001", AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 01, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed },
                     new InboundRequest { InboundRequestId = 2, InboundRequestCode = "REQ-002", AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 05, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed },
                     new InboundRequest { InboundRequestId = 3, InboundRequestCode = "REQ-003", AccountId = Guid.Parse("7e006221-9a70-498d-a0b2-ae587c0cd1e8"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 10, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed },
                     new InboundRequest { InboundRequestId = 4, InboundRequestCode = "REQ-004", AccountId = Guid.Parse("ec57b9d9-680d-4caf-8122-9325352a1e9b"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 15, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed },
                     new InboundRequest { InboundRequestId = 5, InboundRequestCode = "REQ-005", AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 20, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed }
            );

            modelBuilder.Entity<InboundRequestDetails>().HasData(
                     new InboundRequestDetails { InboundRequestDetailsId = 1, InboundRequestId = 1, ProductId = 1, Quantity = 100, UnitPrice = 25, TotalPrice = 2500 },
                     new InboundRequestDetails { InboundRequestDetailsId = 2, InboundRequestId = 1, ProductId = 2, Quantity = 350, UnitPrice = 30.5M, TotalPrice = 10675 },
                     new InboundRequestDetails { InboundRequestDetailsId = 3, InboundRequestId = 2, ProductId = 3, Quantity = 200, UnitPrice = 35, TotalPrice = 7000 },
                     new InboundRequestDetails { InboundRequestDetailsId = 4, InboundRequestId = 3, ProductId = 4, Quantity = 400, UnitPrice = 25.5M, TotalPrice = 10200 },
                     new InboundRequestDetails { InboundRequestDetailsId = 5, InboundRequestId = 4, ProductId = 5, Quantity = 250, UnitPrice = 47.5M, TotalPrice = 11875 }
            );


            modelBuilder.Entity<Inbound>().HasData(
                    new Inbound { InboundId = 1, InboundCode = "INB-001", InboundRequestId = 1, WarehouseId = 1, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 02, 0, 0, 0, DateTimeKind.Utc)),AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Status = InboundStatus.Completed , ProviderId = 1},
                    new Inbound { InboundId = 2, InboundCode = "INB-002", InboundRequestId = 2, WarehouseId = 2, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 06, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Status = InboundStatus.Completed, ProviderId = 2 },
                    new Inbound { InboundId = 3, InboundCode = "INB-003", InboundRequestId = 3, WarehouseId = 1, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 11, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Status = InboundStatus.Completed, ProviderId = 3 },
                    new Inbound { InboundId = 4, InboundCode = "INB-004", InboundRequestId = 4, WarehouseId = 3, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 16, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Status = InboundStatus.Completed , ProviderId = 4 },
                    new Inbound { InboundId = 5, InboundCode = "INB-005", InboundRequestId = 5, WarehouseId = 2, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 21, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Status = InboundStatus.Completed , ProviderId = 5 },
                    new Inbound { InboundId = 6, InboundCode = "INB-006", WarehouseId = 1, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 26, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Status = InboundStatus.Completed, ProviderId =1 },
                    new Inbound { InboundId = 10, InboundCode = "INB-P9", WarehouseId = 1, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 10, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), Status = InboundStatus.Completed, ProviderId = 1, InboundRequestId = null }
                    
                    
                    );
            modelBuilder.Entity<InboundDetails>().HasData(
                new InboundDetails { InboundDetailsId = 1, InboundId = 1, ProductId = 1, Quantity = 100, LotNumber = "LOT-001", OpeningStock = 80, UnitPrice = 25, TotalPrice = 2500, ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 03, 01)), ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 03, 01)) },
                new InboundDetails { InboundDetailsId = 2, InboundId = 1, ProductId = 2, Quantity = 350, LotNumber = "LOT-002", OpeningStock = 100, UnitPrice = 30.5M, TotalPrice = 10675, ExpiryDate = DateOnly.FromDateTime(new DateTime(2027, 06, 15)), ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 06, 15)) },
                new InboundDetails { InboundDetailsId = 3, InboundId = 2, ProductId = 3, Quantity = 200, LotNumber = "LOT-003", OpeningStock = 50, UnitPrice = 35, TotalPrice = 7000, ExpiryDate = DateOnly.FromDateTime(new DateTime(2025, 12, 10)), ManufacturingDate = DateOnly.FromDateTime(new DateTime(2024, 12, 10)) },
                new InboundDetails { InboundDetailsId = 4, InboundId = 3, ProductId = 4, Quantity = 400, LotNumber = "LOT-004", OpeningStock = 60, UnitPrice = 25.5M, TotalPrice = 10200, ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 09, 20)), ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 09, 20)) },
                new InboundDetails { InboundDetailsId = 5, InboundId = 4, ProductId = 5, Quantity = 250, LotNumber = "LOT-005", OpeningStock = 30, UnitPrice = 47.5M, TotalPrice = 11875, ExpiryDate = DateOnly.FromDateTime(new DateTime(2027, 02, 05)), ManufacturingDate = DateOnly.FromDateTime(new DateTime(2026, 02, 05)) },
                new InboundDetails { InboundDetailsId = 6, InboundId = 6, ProductId = 1, Quantity = 30, LotNumber = "LOT-001", OpeningStock = 80, UnitPrice = 25, TotalPrice = 1250, ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 03, 01)), ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 03, 01)) },
                new InboundDetails { InboundDetailsId = 18, InboundId = 10, ProductId = 9, Quantity = 50, LotNumber = "LOT-P9-NHAP", OpeningStock = 0, UnitPrice = 15, TotalPrice = 750, ExpiryDate = DateOnly.FromDateTime(new DateTime(2027, 02, 01)), ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 02, 01)) }


                );

            //Lot
            
            modelBuilder.Entity<Lot>().HasData(
                new Lot { LotId = 1, LotNumber = "LOT-001", ProductId = 1, Quantity = 180, WarehouseId = 1, ProviderId = 1, ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 03, 01)), ExpiryDate = DateOnly.FromDateTime(new DateTime(2025, 03, 01)) },
                new Lot { LotId = 2, LotNumber = "LOT-002", ProductId = 2, Quantity = 450, WarehouseId = 1, ProviderId = 1, ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 06, 15)), ExpiryDate = DateOnly.FromDateTime(new DateTime(2027, 06, 15)) },
                new Lot { LotId = 3, LotNumber = "LOT-003", ProductId = 3, Quantity = 250, WarehouseId = 2, ProviderId = 2, ManufacturingDate = DateOnly.FromDateTime(new DateTime(2024, 12, 10)), ExpiryDate = DateOnly.FromDateTime(new DateTime(2025, 12, 10)) },
                new Lot { LotId = 4, LotNumber = "LOT-004", ProductId = 4, Quantity = 460, WarehouseId = 1, ProviderId = 3, ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 09, 20)), ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 09, 20)) },
                new Lot { LotId = 5, LotNumber = "LOT-005", ProductId = 5, Quantity = 280, WarehouseId = 3, ProviderId = 4, ManufacturingDate = DateOnly.FromDateTime(new DateTime(2026, 02, 05)), ExpiryDate = DateOnly.FromDateTime(new DateTime(2027, 02, 05)) },
                new Lot { LotId = 16, LotNumber = "LOT-P10-KHO2", ProductId = 10, Quantity = 250, WarehouseId = 2, ProviderId = 1, ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 01, 01)), ExpiryDate = DateOnly.FromDateTime(new DateTime(2027, 06, 30)) },
                new Lot { LotId = 15, LotNumber = "LOT-P9-NHAP", ProductId = 9, Quantity = 50, WarehouseId = 1, ProviderId = 1, ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 02, 01)), ExpiryDate = DateOnly.FromDateTime(new DateTime(2027, 02, 01)) }
                );


            modelBuilder.Entity<Outbound>().HasData(
                new Outbound { OutboundId = 1, OutboundCode = "OUT-001", CustomerId = 1, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 03, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") ,Note = "abcxyz"},
                new Outbound { OutboundId = 2, OutboundCode = "OUT-002", CustomerId = 2, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 07, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed , AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new Outbound { OutboundId = 3, OutboundCode = "OUT-003", CustomerId = 3, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 12, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new Outbound { OutboundId = 4, OutboundCode = "OUT-004", CustomerId = 4, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 17, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new Outbound { OutboundId = 5, OutboundCode = "OUT-005", CustomerId = 5, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 22, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed , AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new Outbound { OutboundId = 8, OutboundCode = "OUT-P9-SELL", CustomerId = 3, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 12, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new Outbound { OutboundId = 9, OutboundCode = "OUT-P9-RETURN", CustomerId = 3, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 13, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Returned, AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555") }
                );

            modelBuilder.Entity<OutboundDetails>().HasData(
                new OutboundDetails { OutboundDetailsId = 1, OutboundId = 1, LotId = 1, Quantity = 50, UnitPrice = 25,TotalPrice = 1250},
                new OutboundDetails { OutboundDetailsId = 2, OutboundId = 1, LotId = 2, Quantity = 100, UnitPrice = 30.5M, TotalPrice = 3050 },
                new OutboundDetails { OutboundDetailsId = 3, OutboundId = 2, LotId = 3, Quantity = 75, UnitPrice = 35, TotalPrice = 2625 },
                new OutboundDetails { OutboundDetailsId = 4, OutboundId = 3, LotId = 4, Quantity = 150, UnitPrice = 25.5M, TotalPrice = 3825 },
                new OutboundDetails { OutboundDetailsId = 5, OutboundId = 4, LotId = 5, Quantity = 100, UnitPrice = 47.5M, TotalPrice = 4750 },
                new OutboundDetails { OutboundDetailsId = 19, OutboundId = 8, LotId = 15, Quantity = 7, UnitPrice = 15, TotalPrice = 105 }// LOT-P9-NHAP
                );

            modelBuilder.Entity<LotTransfer>().HasData(
                new LotTransfer { LotTransferId = 1, LotTransferCode = "LT-001", FromWareHouseId = 1, ToWareHouseId = 2, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 03, 0, 0, 0, DateTimeKind.Utc)) },
                new LotTransfer { LotTransferId = 2, LotTransferCode = "LT-002", FromWareHouseId = 1, ToWareHouseId = 3, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 07, 0, 0, 0, DateTimeKind.Utc)) },
                new LotTransfer { LotTransferId = 3, LotTransferCode = "LT-003", FromWareHouseId = 2, ToWareHouseId = 1, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 12, 0, 0, 0, DateTimeKind.Utc)) },
                new LotTransfer { LotTransferId = 4, LotTransferCode = "LT-004", FromWareHouseId = 3, ToWareHouseId = 1, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 17, 0, 0, 0, DateTimeKind.Utc)) },
                new LotTransfer { LotTransferId = 6, LotTransferCode = "LT-P10-IN", FromWareHouseId = 2, ToWareHouseId = 1, AccountId = Guid.Parse("1c4b98f1-e040-42d9-9887-f65011400dd7"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 08, 0, 0, 0, DateTimeKind.Utc)), LotTransferStatus = LotTransferStatus.Completed }
                
                
                
                );
            modelBuilder.Entity<LotTransferDetail>().HasData(
                new LotTransferDetail { LotTransferDetailId = 1, LotTransferId = 1, LotId = 1, Quantity = 50 },
                new LotTransferDetail { LotTransferDetailId = 2, LotTransferId = 1, LotId = 2, Quantity = 100 },
                new LotTransferDetail { LotTransferDetailId = 3, LotTransferId = 2, LotId = 3, Quantity = 75 },
                new LotTransferDetail { LotTransferDetailId = 4, LotTransferId = 3, LotId = 4, Quantity = 150 },
                new LotTransferDetail { LotTransferDetailId = 6, LotTransferId = 6, LotId = 16, Quantity = 250 }
                );

        }
        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }
    }
}
