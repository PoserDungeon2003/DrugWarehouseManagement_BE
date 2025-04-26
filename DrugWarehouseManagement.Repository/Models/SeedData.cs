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
                // Quản trị viên (RoleId = 1)
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), UserName = "admin1", Email = "tranvana@example.com", FullName = "Trần Văn An", PhoneNumber = "0901234567", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "11111111-1111-1111-1111-111111111111", Status = AccountStatus.Active, RoleId = 1 },
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), UserName = "admin2", Email = "levanbinh@example.com", FullName = "Lê Văn Bình", PhoneNumber = "0908765432", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "11111111-1111-1111-1111-111111111112", Status = AccountStatus.Active, RoleId = 1 },
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111113"), UserName = "admin3", Email = "nguyenthucc@example.com", FullName = "Nguyễn Thúc Cường", PhoneNumber = "0912345678", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "11111111-1111-1111-1111-111111111113", Status = AccountStatus.Active, RoleId = 1 },
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111114"), UserName = "admin4", Email = "phamthid@example.com", FullName = "Phạm Thị Diệu", PhoneNumber = "0934567890", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "11111111-1111-1111-1111-111111111114", Status = AccountStatus.Active, RoleId = 1 },
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111115"), UserName = "admin5", Email = "hovanoe@example.com", FullName = "Hồ Văn Em", PhoneNumber = "0978901234", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "11111111-1111-1111-1111-111111111115", Status = AccountStatus.Active, RoleId = 1 },

                // Quản lý kho (RoleId = 2)
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222221"), UserName = "manager1", Email = "vuongthif@example.com", FullName = "Vương Thị Phượng", PhoneNumber = "0909876543", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222221", Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), UserName = "manager2", Email = "dinhvangg@example.com", FullName = "Đinh Văn Giang", PhoneNumber = "0911223344", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222222", Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222223"), UserName = "manager3", Email = "tranthanhh@example.com", FullName = "Trần Thanh Hải", PhoneNumber = "0933445566", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222223", Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222224"), UserName = "manager4", Email = "leminhi@example.com", FullName = "Lê Minh Hoàng", PhoneNumber = "0977889900", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222224", Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222225"), UserName = "manager5", Email = "phambaoj@example.com", FullName = "Phạm Bảo Châu", PhoneNumber = "0906543210", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222225", Status = AccountStatus.Active, RoleId = 2 },

                // Kế toán (RoleId = 3)
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333331"), UserName = "accountant1", Email = "nguyenmaik@example.com", FullName = "Nguyễn Mai Khanh", PhoneNumber = "0919283746", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333331", Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333332"), UserName = "accountant2", Email = "dovietl@example.com", FullName = "Đỗ Việt Long", PhoneNumber = "0935791324", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333332", Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), UserName = "accountant3", Email = "lethim@example.com", FullName = "Lê Thị Minh", PhoneNumber = "0971357924", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333333", Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333334"), UserName = "accountant4", Email = "hoangvann@example.com", FullName = "Hoàng Văn Nam", PhoneNumber = "0902468135", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333334", Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333335"), UserName = "accountant5", Email = "trantoan@example.com", FullName = "Trần Toàn", PhoneNumber = "0938527419", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333335", Status = AccountStatus.Active, RoleId = 3 },

                // Quản lý bán hàng (RoleId = 4)
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), UserName = "saleadmin1", Email = "nguyenphuongp@example.com", FullName = "Nguyễn Phương Anh", PhoneNumber = "0914725836", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444441", Status = AccountStatus.Active, RoleId = 4 },
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), UserName = "saleadmin2", Email = "lehoangq@example.com", FullName = "Lê Hoàng Quân", PhoneNumber = "0936987412", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444442", Status = AccountStatus.Active, RoleId = 4 },
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444443"), UserName = "saleadmin3", Email = "phamthir@example.com", FullName = "Phạm Thị Hồng", PhoneNumber = "0978521496", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444443", Status = AccountStatus.Active, RoleId = 4 },
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), UserName = "saleadmin4", Email = "dinhvans@example.com", FullName = "Đinh Văn Sơn", PhoneNumber = "0905123678", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444444", Status = AccountStatus.Active, RoleId = 4 },
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444445"), UserName = "saleadmin5", Email = "nguyentuant@example.com", FullName = "Nguyễn Tuấn Tú", PhoneNumber = "0939876541", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444445", Status = AccountStatus.Active, RoleId = 4 },

                // Giám đốc (RoleId = 5)
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555551"), UserName = "director1", Email = "levanu@example.com", FullName = "Lê Văn Út", PhoneNumber = "0916357892", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555551", Status = AccountStatus.Active, RoleId = 5 },
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555552"), UserName = "director2", Email = "phamthiv@example.com", FullName = "Phạm Thị Vân", PhoneNumber = "0937485961", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555552", Status = AccountStatus.Active, RoleId = 5 },
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555553"), UserName = "director3", Email = "trinhvanx@example.com", FullName = "Trịnh Văn Xuân", PhoneNumber = "0979632581", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555553", Status = AccountStatus.Active, RoleId = 5 },
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555554"), UserName = "director4", Email = "dohongy@example.com", FullName = "Đỗ Hồng Yến", PhoneNumber = "0908254796", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555554", Status = AccountStatus.Active, RoleId = 5 },
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), UserName = "director5", Email = "nguyenhoangz@example.com", FullName = "Nguyễn Hoàng Duy", PhoneNumber = "0931598742", PasswordHash = HashPassword("SecurePassword1!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555555", Status = AccountStatus.Active, RoleId = 5 }
            );
            // 3. Seed Providers
            modelBuilder.Entity<Provider>().HasData(
                new Provider { ProviderId = 1, ProviderName = "Công ty CP Dược phẩm OPC", Address = "1017 Hồng Bàng, Phường 12, Quận 6, TP.HCM", PhoneNumber = "02837517111", TaxCode = "0300369857", Email = "info@opcpharma.com", Status = ProviderStatus.Active, DocumentNumber = "OPC-HCM-250409-001" },
                new Provider { ProviderId = 2, ProviderName = "Công ty CP Dược phẩm Imexpharm - Chi nhánh TP.HCM", Address = "Số 4 Nguyễn Thị Minh Khai, Phường Đa Kao, Quận 1, TP.HCM", PhoneNumber = "02838223637", TaxCode = "1400113776-001", Email = "hcm.branch@imexpharm.com", Status = ProviderStatus.Active, DocumentNumber = "IMP-HCM-090425-002" },
                new Provider { ProviderId = 3, ProviderName = "Công ty TNHH MTV Dược phẩm DHG - Chi nhánh TP.HCM", Address = "288 Bis Nguyễn Văn Trỗi, Phường 15, Quận Phú Nhuận, TP.HCM", PhoneNumber = "02838443114", TaxCode = "1800154789-002", Email = "dhghcm@dhgpharma.com.vn", Status = ProviderStatus.Active, DocumentNumber = "DHG-HCM-2025-003" },
                new Provider { ProviderId = 4, ProviderName = "Công ty CP Pymepharco - Chi nhánh TP.HCM", Address = "Tầng 5, Tòa nhà Pearl Plaza, 561A Điện Biên Phủ, Phường 25, Quận Bình Thạnh, TP.HCM", PhoneNumber = "02839708789", TaxCode = "4400236473-003", Email = "hcm.sales@pymepharco.com", Status = ProviderStatus.Active, DocumentNumber = "PYM-HCM-040925-004" },
                new Provider { ProviderId = 5, ProviderName = "Công ty CP Dược phẩm Savi", Address = "Lô J2-J3-J4, Đường D3, KCN Tây Bắc Củ Chi, TP.HCM", PhoneNumber = "02837260288", TaxCode = "0302589901", Email = "info@savipharm.com", Status = ProviderStatus.Active, DocumentNumber = "SAVI-HCM-250409-005" }
            );
            modelBuilder.Entity<Categories>().HasData(
                // Nhóm chính: Báo cáo
                new Categories { CategoriesId = (int)SystemConfigEnum.ReportId, CategoryName = "Báo cáo", ParentCategoryId = null, Description = "Các loại báo cáo thống kê và phân tích.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 101, CategoryName = "Báo cáo Doanh thu", ParentCategoryId = 100, Description = "Báo cáo về doanh thu bán hàng.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 102, CategoryName = "Báo cáo Kho", ParentCategoryId = 100, Description = "Báo cáo về tình trạng và số lượng hàng tồn kho.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 103, CategoryName = "Báo cáo Bán hàng", ParentCategoryId = 100, Description = "Báo cáo chi tiết về các giao dịch bán hàng.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 104, CategoryName = "Báo cáo Lợi nhuận", ParentCategoryId = 100, Description = "Báo cáo về lợi nhuận thu được.", Status = CategoriesStatus.Active },

                // Nhóm chính: Thuốc & Dược phẩm
                new Categories { CategoriesId = (int)SystemConfigEnum.MedicineId, CategoryName = "Thuốc & Dược phẩm", ParentCategoryId = null, Description = "Các loại thuốc và dược phẩm.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 201, CategoryName = "Thuốc kê đơn", ParentCategoryId = 200, Description = "Thuốc cần có đơn thuốc của bác sĩ.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 205, CategoryName = "Kháng sinh", ParentCategoryId = 200, Description = "Thuốc dùng để điều trị nhiễm khuẩn.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 206, CategoryName = "Kháng sinh phổ rộng", ParentCategoryId = 200, Description = "Kháng sinh có tác dụng trên nhiều loại vi khuẩn.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 202, CategoryName = "Thuốc không kê đơn", ParentCategoryId = 200, Description = "Thuốc có thể mua tự do không cần đơn thuốc.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 203, CategoryName = "Thuốc giảm đau, hạ sốt", ParentCategoryId = 200, Description = "Thuốc giúp giảm đau và hạ sốt.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 204, CategoryName = "Thuốc trị cảm cúm", ParentCategoryId = 200, Description = "Thuốc điều trị các triệu chứng cảm lạnh và cúm.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 207, CategoryName = "Vitamin & Thực phẩm chức năng", ParentCategoryId = 200, Description = "Các sản phẩm bổ sung vitamin và khoáng chất.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 208, CategoryName = "Vitamin tổng hợp", ParentCategoryId = 200, Description = "Các loại vitamin chứa nhiều dưỡng chất.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 209, CategoryName = "Men vi sinh & Hỗ trợ tiêu hóa", ParentCategoryId = 200, Description = "Các sản phẩm chứa lợi khuẩn và hỗ trợ tiêu hóa.", Status = CategoriesStatus.Active },

                // Nhóm chính: Thiết bị Y tế
                new Categories { CategoriesId = (int)SystemConfigEnum.MedicalEquipmentId, CategoryName = "Thiết bị Y tế", ParentCategoryId = null, Description = "Các thiết bị sử dụng cho mục đích y tế.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 301, CategoryName = "Dụng cụ Chẩn đoán & Theo dõi", ParentCategoryId = 300, Description = "Thiết bị dùng cho việc chẩn đoán và theo dõi sức khỏe.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 302, CategoryName = "Máy đo huyết áp", ParentCategoryId = 300, Description = "Thiết bị đo huyết áp.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 303, CategoryName = "Nhiệt kế điện tử", ParentCategoryId = 300, Description = "Thiết bị đo nhiệt độ cơ thể điện tử.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 304, CategoryName = "Máy đo đường huyết", ParentCategoryId = 300, Description = "Thiết bị đo lượng đường trong máu.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 305, CategoryName = "Vật tư Tiêu hao", ParentCategoryId = 300, Description = "Các vật tư sử dụng một lần trong y tế.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 306, CategoryName = "Băng gạc & Vật liệu băng bó", ParentCategoryId = 300, Description = "Các loại băng, gạc và vật liệu dùng để băng bó vết thương.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 307, CategoryName = "Kim tiêm & Ống tiêm", ParentCategoryId = 300, Description = "Các loại kim và ống tiêm.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 308, CategoryName = "Thiết bị Hỗ trợ Vận động", ParentCategoryId = 300, Description = "Các thiết bị hỗ trợ người có vấn đề về vận động.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 309, CategoryName = "Nạng & Gậy", ParentCategoryId = 300, Description = "Các loại nạng và gậy chống.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 310, CategoryName = "Xe lăn", ParentCategoryId = 300, Description = "Các loại xe lăn cho người khuyết tật hoặc người già.", Status = CategoriesStatus.Active },

                // Nhóm chính: Chăm sóc Cá nhân & Làm đẹp
                new Categories { CategoriesId = (int)SystemConfigEnum.BeautyId, CategoryName = "Chăm sóc Cá nhân & Làm đẹp", ParentCategoryId = null, Description = "Các sản phẩm chăm sóc cá nhân và làm đẹp.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 401, CategoryName = "Chăm sóc Da mặt", ParentCategoryId = 400, Description = "Các sản phẩm chăm sóc da mặt.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 402, CategoryName = "Sữa rửa mặt", ParentCategoryId = 400, Description = "Các loại sữa rửa mặt.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 403, CategoryName = "Kem dưỡng da", ParentCategoryId = 400, Description = "Các loại kem dưỡng da.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 404, CategoryName = "Serum & Tinh chất", ParentCategoryId = 400, Description = "Các loại serum và tinh chất dưỡng da.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 405, CategoryName = "Chăm sóc Cơ thể", ParentCategoryId = 400, Description = "Các sản phẩm chăm sóc cơ thể.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 406, CategoryName = "Sữa tắm", ParentCategoryId = 400, Description = "Các loại sữa tắm.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 407, CategoryName = "Kem dưỡng thể", ParentCategoryId = 400, Description = "Các loại kem dưỡng thể.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 408, CategoryName = "Chăm sóc Tóc", ParentCategoryId = 400, Description = "Các sản phẩm chăm sóc tóc.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 409, CategoryName = "Dầu gội", ParentCategoryId = 400, Description = "Các loại dầu gội.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 410, CategoryName = "Dầu xả", ParentCategoryId = 400, Description = "Các loại dầu xả.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 411, CategoryName = "Sản phẩm Chống nắng", ParentCategoryId = 400, Description = "Các sản phẩm bảo vệ da khỏi tác hại của ánh nắng mặt trời.", Status = CategoriesStatus.Active },

                // Nhóm chính: Sản phẩm cho Mẹ & Bé
                new Categories { CategoriesId = (int)SystemConfigEnum.MomToolId, CategoryName = "Sản phẩm cho Mẹ & Bé", ParentCategoryId = null, Description = "Các sản phẩm dành cho bà mẹ và trẻ em.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 501, CategoryName = "Sản phẩm cho Mẹ", ParentCategoryId = 500, Description = "Các sản phẩm dành cho phụ nữ mang thai và sau sinh.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 502, CategoryName = "Vitamin & Thực phẩm chức năng cho mẹ", ParentCategoryId = 500, Description = "Vitamin và thực phẩm chức năng dành cho mẹ.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 503, CategoryName = "Đồ dùng cho mẹ", ParentCategoryId = 500, Description = "Các đồ dùng cá nhân cho mẹ.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 504, CategoryName = "Sản phẩm cho Bé", ParentCategoryId = 500, Description = "Các sản phẩm dành cho trẻ em.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 505, CategoryName = "Sữa & Thực phẩm cho bé", ParentCategoryId = 500, Description = "Các loại sữa và thực phẩm dinh dưỡng cho trẻ em.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 506, CategoryName = "Đồ dùng cho bé", ParentCategoryId = 500, Description = "Các đồ dùng cá nhân cho trẻ em.", Status = CategoriesStatus.Active },

                // Nhóm chính: Khác
                new Categories { CategoriesId = (int)SystemConfigEnum.OtherId, CategoryName = "Khác", ParentCategoryId = null, Description = "Các danh mục sản phẩm khác.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 601, CategoryName = "Vật tư Y tế Gia đình", ParentCategoryId = 600, Description = "Các vật tư y tế sử dụng tại nhà.", Status = CategoriesStatus.Active },
                    new Categories { CategoriesId = 602, CategoryName = "Sản phẩm Hỗ trợ Sức khỏe", ParentCategoryId = 600, Description = "Các sản phẩm hỗ trợ sức khỏe tổng thể.", Status = CategoriesStatus.Active },

                new Categories { CategoriesId = (int)SystemConfigEnum.SKUId, CategoryName = "Đơn vị tính", ParentCategoryId = null, Description = "Các danh mục sản phẩm khác.", Status = CategoriesStatus.Active }
            );

            // 4. Seed Warehouses
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { WarehouseId = 1, WarehouseCode = "KVN-01", WarehouseName = "Kho Việt Nam", Address = "Số 10 Đường Cộng Hòa, Phường 13, Quận Tân Bình, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "K20250409-001" },
                new Warehouse { WarehouseId = 2, WarehouseCode = "KHUY-01", WarehouseName = "Kho Hủy", Address = "Khu vực xử lý hàng lỗi, Đường Số 7, KCN Vĩnh Lộc, Bình Chánh, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "KH20250409-002" },
                new Warehouse { WarehouseId = 3, WarehouseCode = "KTHU-01", WarehouseName = "Kho Thuốc", Address = "Số 3B Đường Nguyễn Văn Quá, Đông Hưng Thuận, Quận 12, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "KT20250409-003" },
                new Warehouse { WarehouseId = 4, WarehouseCode = "KMP-01", WarehouseName = "Kho Mỹ Phẩm", Address = "Số 1 Lê Duẩn, Bến Nghé, Quận 1, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "KMP20250409-004" },
                new Warehouse { WarehouseId = 5, WarehouseCode = "KTH-01", WarehouseName = "Kho Trung Hạnh", Address = "Số 88 Đường 3 Tháng 2, Phường 11, Quận 10, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "KTH20250409-005" },
                new Warehouse { WarehouseId = 6, WarehouseCode = "KTAMTHOI-01", WarehouseName = "Kho Tạm", Address = "Số 95 Đường 3 Tháng 2, Phường 11, Quận 10, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "KTAMTHOI20250409-006" }
            );

            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, CustomerName = "Nguyễn Văn An", Address = "123 Đường Cộng Hòa, Phường 12, Quận Tân Bình, TP. Hồ Chí Minh", PhoneNumber = "0903123456", Email = "nguyen.van.an@gmail.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "KH-NVAN-250409-001" },
                new Customer { CustomerId = 2, CustomerName = "Lê Thị Bình", Address = "456 Đường 3 Tháng 2, Phường 10, Quận 10, TP. Hồ Chí Minh", PhoneNumber = "0938987654", Email = "le.thi.binh79@yahoo.com.vn", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "KH-LTBI-250409-002" },
                new Customer { CustomerId = 3, CustomerName = "Trần Minh Cường", Address = "789 Đường Nguyễn Trãi, Phường 8, Quận 5, TP. Hồ Chí Minh", PhoneNumber = "0919234567", Email = "minhcuong.tran@fpt.net", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "KH-TMCO-250409-003" },
                new Customer { CustomerId = 4, CustomerName = "Phạm Thị Diệu Hương", Address = "321 Đường Lê Văn Sỹ, Phường 13, Quận Phú Nhuận, TP. Hồ Chí Minh", PhoneNumber = "0977889900", Email = "dieuhuong.pham@vnn.vn", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "KH-PTDH-250409-004" },
                new Customer { CustomerId = 5, CustomerName = "Hoàng Quốc Việt", Address = "654 Đường Điện Biên Phủ, Phường 11, Quận 3, TP. Hồ Chí Minh", PhoneNumber = "0908555666", Email = "hoang.viet.quoc@outlook.com", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "KH-HQVI-250409-005" },
                new Customer { CustomerId = 6, CustomerName = "Vũ Ngọc Ánh", Address = "987 Đường Cách Mạng Tháng 8, Phường 15, Quận 10, TP. Hồ Chí Minh", PhoneNumber = "0935123456", Email = "ngocanh.vu@gmail.com", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "KH-VNAN-250409-006" },
                new Customer { CustomerId = 7, CustomerName = "Đặng Hoàng Long", Address = "159 Đường Nguyễn Thị Thập, Phường Tân Hưng, Quận 7, TP. Hồ Chí Minh", PhoneNumber = "0917890123", Email = "hoanglong.dang@saigonnet.vn", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "KH-DHLO-250409-007" },
                new Customer { CustomerId = 8, CustomerName = "Bùi Thị Thủy Tiên", Address = "753 Đường Phan Xích Long, Phường 2, Quận Phú Nhuận, TP. Hồ Chí Minh", PhoneNumber = "0909456789", Email = "thuytien.bui@hcm.fpt.vn", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "KH-BTTH-250409-008" },
                new Customer { CustomerId = 9, CustomerName = "Lâm Chấn Khang", Address = "852 Đường Kinh Dương Vương, Phường An Lạc, Quận Bình Tân, TP. Hồ Chí Minh", PhoneNumber = "0933224466", Email = "chankhang.lam@vitanet.vn", IsLoyal = true, Status = CustomerStatus.Active, DocumentNumber = "KH-LCKH-250409-009" },
                new Customer { CustomerId = 10, CustomerName = "Trương Thị Mỹ Linh", Address = "951 Đường Trần Hưng Đạo, Phường 1, Quận 5, TP. Hồ Chí Minh", PhoneNumber = "0976543210", Email = "mylinh.truong@hcmtelecom.vn", IsLoyal = false, Status = CustomerStatus.Active, DocumentNumber = "KH-TTML-250409-010" }
            );

            modelBuilder.Entity<Inbound>().HasData(
    new Inbound
    {
        InboundId = 1,
        InboundCode = "INB-2024-001",
        ProviderOrderCode = "PO-2024-001",
        Note = "First quarter medical supplies",
        InboundDate = Instant.FromUtc(2024, 1, 5, 9, 30, 0),
        Status = InboundStatus.Completed,
        ProviderId = 1,
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333331"), // Replace with actual account ID
        WarehouseId = 1
    },
    new Inbound
    {
        InboundId = 2,
        InboundCode = "INB-2024-002",
        ProviderOrderCode = "PO-2024-002",
        Note = "Second quarter medical supplies",
        InboundDate = Instant.FromUtc(2024, 6, 10, 14, 15, 0),
        Status = InboundStatus.Completed,
        ProviderId = 1,
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333331"), // Replace with actual account ID
        WarehouseId = 1
    },
    new Inbound
    {
        InboundId = 3,
        InboundCode = "INB-2024-003",
        ProviderOrderCode = "PO-2024-003",
        Note = "Third quarter medical supplies",
        InboundDate = Instant.FromUtc(2024, 7, 20, 11, 0, 0),
        Status = InboundStatus.Completed,
        ProviderId = 2,
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333331"), // Replace with actual account ID
        WarehouseId = 6
    },
    new Inbound
    {
        InboundId = 4,
        InboundCode = "INB-2024-004",
        ProviderOrderCode = "PO-2024-004",
        Note = "Additional Ibuprofen stock",
        InboundDate = Instant.FromUtc(2024, 6, 5, 10, 30, 0),
        Status = InboundStatus.Completed,
        ProviderId = 1,
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333331"), // Replace with actual account ID
        WarehouseId = 3
    },
    new Inbound
    {
        InboundId = 5,
        InboundCode = "INB-2024-005",
        ProviderOrderCode = "PO-2024-005",
        Note = "Additional Ibuprofen variants",
        InboundDate = Instant.FromUtc(2024, 6, 7, 9, 45, 0),
        Status = InboundStatus.Completed,
        ProviderId = 1,
        AccountId = Guid.Parse("33333333-3333-3333-3333-333333333331"), // Replace with actual account ID
        WarehouseId = 4
    }
);

            // Seed data for InboundDetails
            modelBuilder.Entity<InboundDetails>().HasData(
                // InboundId 1 - Paracetamol (matches Lot with LotId 1)
                new InboundDetails
                {
                    InboundDetailsId = 1,
                    LotNumber = "EXP-001",
                    ManufacturingDate = new DateOnly(2024, 1, 1),
                    ExpiryDate = new DateOnly(2025, 4, 20),
                    ProductId = 1, // Paracetamol 500mg
                    Quantity = 100,
                    UnitPrice = 0.25m,
                    OpeningStock = 0,
                    TotalPrice = 25.00m,
                    InboundId = 1
                },

                // InboundId 2 - Contains Ibuprofen 200mg (matches Lot with LotId 2)
                new InboundDetails
                {
                    InboundDetailsId = 2,
                    LotNumber = "WH2-001",
                    ManufacturingDate = new DateOnly(2024, 6, 1),
                    ExpiryDate = new DateOnly(2026, 6, 1),
                    ProductId = 2, // Ibuprofen 200mg
                    Quantity = 50,
                    UnitPrice = 0.30m,
                    OpeningStock = 0,
                    TotalPrice = 15.00m,
                    InboundId = 2
                },

                // InboundId 2 - Contains Ibuprofen 600mg (matches Lot with LotId 7)
                new InboundDetails
                {
                    InboundDetailsId = 3,
                    LotNumber = "WH7-001",
                    ManufacturingDate = new DateOnly(2024, 6, 1),
                    ExpiryDate = new DateOnly(2026, 6, 1),
                    ProductId = 6, // Ibuprofen 600mg
                    Quantity = 50,
                    UnitPrice = 0.45m,
                    OpeningStock = 0,
                    TotalPrice = 22.50m,
                    InboundId = 2
                },

                // InboundId 2 - Contains Ibuprofen 700mg (matches Lot with LotId 8)
                new InboundDetails
                {
                    InboundDetailsId = 4,
                    LotNumber = "WH8-001",
                    ManufacturingDate = new DateOnly(2024, 6, 1),
                    ExpiryDate = new DateOnly(2026, 6, 1),
                    ProductId = 7, // Ibuprofen 700mg
                    Quantity = 50,
                    UnitPrice = 0.50m,
                    OpeningStock = 0,
                    TotalPrice = 25.00m,
                    InboundId = 2
                },

                // InboundId 3 - Contains Amoxicillin (matches Lot with LotId 3)
                new InboundDetails
                {
                    InboundDetailsId = 5,
                    LotNumber = "WH3-001",
                    ManufacturingDate = new DateOnly(2024, 7, 15),
                    ExpiryDate = new DateOnly(2026, 7, 15),
                    ProductId = 3, // Amoxicillin 250mg
                    Quantity = 75,
                    UnitPrice = 0.60m,
                    OpeningStock = 0,
                    TotalPrice = 45.00m,
                    InboundId = 3
                },

                // InboundId 4 - Contains Ibuprofen 400mg (matches Lot with LotId 5)
                new InboundDetails
                {
                    InboundDetailsId = 6,
                    LotNumber = "WH5-001",
                    ManufacturingDate = new DateOnly(2024, 6, 1),
                    ExpiryDate = new DateOnly(2026, 6, 1),
                    ProductId = 4, // Ibuprofen 400mg
                    Quantity = 50,
                    UnitPrice = 0.35m,
                    OpeningStock = 0,
                    TotalPrice = 17.50m,
                    InboundId = 4
                },

                // InboundId 5 - Contains Ibuprofen 500mg (matches Lot with LotId 6)
                new InboundDetails
                {
                    InboundDetailsId = 7,
                    LotNumber = "WH6-001",
                    ManufacturingDate = new DateOnly(2024, 6, 1),
                    ExpiryDate = new DateOnly(2026, 6, 1),
                    ProductId = 5, // Ibuprofen 500mg
                    Quantity = 50,
                    UnitPrice = 0.40m,
                    OpeningStock = 0,
                    TotalPrice = 20.00m,
                    InboundId = 5
                }
            );
            modelBuilder.Entity<Lot>().HasData(
    // 1) Expired lot (ExpiryDate < today)
    new Lot
    {
        LotId = 1,
        Quantity = 100,
        LotNumber = "EXP-001",
        ManufacturingDate = new DateOnly(2024, 1, 1),
        ExpiryDate = new DateOnly(2025, 4, 20),  // expired (today is 2025-04-25)
        WarehouseId = 1,    // valid warehouse
        ProviderId = 1,
        ProductId = 1
    },

    // 2) Lot in forbidden warehouse #2
    new Lot
    {
        LotId = 2,
        Quantity = 50,
        LotNumber = "WH2-001",
        ManufacturingDate = new DateOnly(2024, 6, 1),
        ExpiryDate = new DateOnly(2026, 6, 1),   // still valid
        WarehouseId = 1,
        ProviderId = 1,
        ProductId = 2
    },
     new Lot
     {
         LotId = 5,
         Quantity = 50,
         LotNumber = "WH5-001",
         ManufacturingDate = new DateOnly(2024, 6, 1),
         ExpiryDate = new DateOnly(2026, 6, 1),   // still valid
         WarehouseId = 3,
         ProviderId = 1,
         ProductId = 4
     },
      new Lot
      {
          LotId = 6,
          Quantity = 50,
          LotNumber = "WH6-001",
          ManufacturingDate = new DateOnly(2024, 6, 1),
          ExpiryDate = new DateOnly(2026, 6, 1),   // still valid
          WarehouseId = 4,
          ProviderId = 1,
          ProductId = 5
      },
       new Lot
       {
           LotId = 7,
           Quantity = 50,
           LotNumber = "WH7-001",
           ManufacturingDate = new DateOnly(2024, 6, 1),
           ExpiryDate = new DateOnly(2026, 6, 1),   // still valid
           WarehouseId = 1,
           ProviderId = 1,
           ProductId = 6
       },
        new Lot
        {
            LotId = 8,
            Quantity = 50,
            LotNumber = "WH8-001",
            ManufacturingDate = new DateOnly(2024, 6, 1),
            ExpiryDate = new DateOnly(2026, 6, 1),   // still valid
            WarehouseId = 1,
            ProviderId = 1,
            ProductId = 7
        },
    // 3) Lot in forbidden warehouse #6
    new Lot
    {
        LotId = 3,
        Quantity = 75,
        LotNumber = "WH3-001",
        ManufacturingDate = new DateOnly(2024, 7, 15),
        ExpiryDate = new DateOnly(2026, 7, 15),  // still valid
        WarehouseId = 6,    // another forbidden
        ProviderId = 2,
        ProductId = 3
    }

);
            modelBuilder.Entity<Product>().HasData(
    new Product
    {
        ProductId = 1,
        ProductName = "Paracetamol 500mg",
        ProductCode = "PRC-500",
        SKU = "PARA-500",
        MadeFrom = "Acetaminophen",
        Status = ProductStatus.Active
    },
    new Product
    {
        ProductId = 2,
        ProductName = "Ibuprofen 200mg",
        ProductCode = "IBU-200",
        SKU = "IBUP-200",
        MadeFrom = "Ibuprofen",
        Status = ProductStatus.Active
    },
     new Product
     {
         ProductId = 4,
         ProductName = "Ibuprofen 400mg",
         ProductCode = "IBU-400",
         SKU = "IBUP-400",
         MadeFrom = "Ibuprofen",
         Status = ProductStatus.Active
     },
      new Product
      {
          ProductId = 5,
          ProductName = "Ibuprofen 500mg",
          ProductCode = "IBU-500",
          SKU = "IBUP-500",
          MadeFrom = "Ibuprofen",
          Status = ProductStatus.Active
      },
       new Product
       {
           ProductId = 6,
           ProductName = "Ibuprofen 600mg",
           ProductCode = "IBU-600",
           SKU = "IBUP-600",
           MadeFrom = "Ibuprofen",
           Status = ProductStatus.Active
       },
        new Product
        {
            ProductId = 7,
            ProductName = "Ibuprofen 700mg",
            ProductCode = "IBU-700",
            SKU = "IBUP-700",
            MadeFrom = "Ibuprofen",
            Status = ProductStatus.Active
        },
    new Product
    {
        ProductId = 3,
        ProductName = "Amoxicillin 250mg",
        ProductCode = "AMX-250",
        SKU = "AMOX-250",
        MadeFrom = "Amoxicillin",
        Status = ProductStatus.Active
    }
);
        }
        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }
    }
}
