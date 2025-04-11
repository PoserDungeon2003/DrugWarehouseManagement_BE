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
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), UserName = "admin2", Email = "levanbinh@example.com", FullName = "Lê Văn Bình", PhoneNumber = "0908765432", PasswordHash = HashPassword("SecurePassword2!"), ConcurrencyStamp = "11111111-1111-1111-1111-111111111112", Status = AccountStatus.Active, RoleId = 1 },
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111113"), UserName = "admin3", Email = "nguyenthucc@example.com", FullName = "Nguyễn Thúc Cường", PhoneNumber = "0912345678", PasswordHash = HashPassword("SecurePassword3!"), ConcurrencyStamp = "11111111-1111-1111-1111-111111111113", Status = AccountStatus.Active, RoleId = 1 },
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111114"), UserName = "admin4", Email = "phamthid@example.com", FullName = "Phạm Thị Diệu", PhoneNumber = "0934567890", PasswordHash = HashPassword("SecurePassword4!"), ConcurrencyStamp = "11111111-1111-1111-1111-111111111114", Status = AccountStatus.Active, RoleId = 1 },
                new Account { Id = Guid.Parse("11111111-1111-1111-1111-111111111115"), UserName = "admin5", Email = "hovanoe@example.com", FullName = "Hồ Văn Em", PhoneNumber = "0978901234", PasswordHash = HashPassword("SecurePassword5!"), ConcurrencyStamp = "11111111-1111-1111-1111-111111111115", Status = AccountStatus.Active, RoleId = 1 },

                // Quản lý kho (RoleId = 2)
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222221"), UserName = "manager1", Email = "vuongthif@example.com", FullName = "Vương Thị Phượng", PhoneNumber = "0909876543", PasswordHash = HashPassword("SecurePassword6!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222221", Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), UserName = "manager2", Email = "dinhvangg@example.com", FullName = "Đinh Văn Giang", PhoneNumber = "0911223344", PasswordHash = HashPassword("SecurePassword7!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222222", Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222223"), UserName = "manager3", Email = "tranthanhh@example.com", FullName = "Trần Thanh Hải", PhoneNumber = "0933445566", PasswordHash = HashPassword("SecurePassword8!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222223", Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222224"), UserName = "manager4", Email = "leminhi@example.com", FullName = "Lê Minh Hoàng", PhoneNumber = "0977889900", PasswordHash = HashPassword("SecurePassword9!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222224", Status = AccountStatus.Active, RoleId = 2 },
                new Account { Id = Guid.Parse("22222222-2222-2222-2222-222222222225"), UserName = "manager5", Email = "phambaoj@example.com", FullName = "Phạm Bảo Châu", PhoneNumber = "0906543210", PasswordHash = HashPassword("SecurePassword10!"), ConcurrencyStamp = "22222222-2222-2222-2222-222222222225", Status = AccountStatus.Active, RoleId = 2 },

                // Kế toán (RoleId = 3)
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333331"), UserName = "accountant1", Email = "nguyenmaik@example.com", FullName = "Nguyễn Mai Khanh", PhoneNumber = "0919283746", PasswordHash = HashPassword("SecurePassword11!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333331", Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333332"), UserName = "accountant2", Email = "dovietl@example.com", FullName = "Đỗ Việt Long", PhoneNumber = "0935791324", PasswordHash = HashPassword("SecurePassword12!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333332", Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), UserName = "accountant3", Email = "lethim@example.com", FullName = "Lê Thị Minh", PhoneNumber = "0971357924", PasswordHash = HashPassword("SecurePassword13!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333333", Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333334"), UserName = "accountant4", Email = "hoangvann@example.com", FullName = "Hoàng Văn Nam", PhoneNumber = "0902468135", PasswordHash = HashPassword("SecurePassword14!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333334", Status = AccountStatus.Active, RoleId = 3 },
                new Account { Id = Guid.Parse("33333333-3333-3333-3333-333333333335"), UserName = "accountant5", Email = "trantoan@example.com", FullName = "Trần Toàn", PhoneNumber = "0938527419", PasswordHash = HashPassword("SecurePassword15!"), ConcurrencyStamp = "33333333-3333-3333-3333-333333333335", Status = AccountStatus.Active, RoleId = 3 },

                // Quản lý bán hàng (RoleId = 4)
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), UserName = "saleadmin1", Email = "nguyenphuongp@example.com", FullName = "Nguyễn Phương Anh", PhoneNumber = "0914725836", PasswordHash = HashPassword("SecurePassword16!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444441", Status = AccountStatus.Active, RoleId = 4 },
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), UserName = "saleadmin2", Email = "lehoangq@example.com", FullName = "Lê Hoàng Quân", PhoneNumber = "0936987412", PasswordHash = HashPassword("SecurePassword17!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444442", Status = AccountStatus.Active, RoleId = 4 },
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444443"), UserName = "saleadmin3", Email = "phamthir@example.com", FullName = "Phạm Thị Hồng", PhoneNumber = "0978521496", PasswordHash = HashPassword("SecurePassword18!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444443", Status = AccountStatus.Active, RoleId = 4 },
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), UserName = "saleadmin4", Email = "dinhvans@example.com", FullName = "Đinh Văn Sơn", PhoneNumber = "0905123678", PasswordHash = HashPassword("SecurePassword19!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444444", Status = AccountStatus.Active, RoleId = 4 },
                new Account { Id = Guid.Parse("44444444-4444-4444-4444-444444444445"), UserName = "saleadmin5", Email = "nguyentuant@example.com", FullName = "Nguyễn Tuấn Tú", PhoneNumber = "0939876541", PasswordHash = HashPassword("SecurePassword20!"), ConcurrencyStamp = "44444444-4444-4444-4444-444444444445", Status = AccountStatus.Active, RoleId = 4 },

                // Giám đốc (RoleId = 5)
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555551"), UserName = "director1", Email = "levanu@example.com", FullName = "Lê Văn Út", PhoneNumber = "0916357892", PasswordHash = HashPassword("SecurePassword21!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555551", Status = AccountStatus.Active, RoleId = 5 },
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555552"), UserName = "director2", Email = "phamthiv@example.com", FullName = "Phạm Thị Vân", PhoneNumber = "0937485961", PasswordHash = HashPassword("SecurePassword22!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555552", Status = AccountStatus.Active, RoleId = 5 },
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555553"), UserName = "director3", Email = "trinhvanx@example.com", FullName = "Trịnh Văn Xuân", PhoneNumber = "0979632581", PasswordHash = HashPassword("SecurePassword23!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555553", Status = AccountStatus.Active, RoleId = 5 },
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555554"), UserName = "director4", Email = "dohongy@example.com", FullName = "Đỗ Hồng Yến", PhoneNumber = "0908254796", PasswordHash = HashPassword("SecurePassword24!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555554", Status = AccountStatus.Active, RoleId = 5 },
                new Account { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), UserName = "director5", Email = "nguyenhoangz@example.com", FullName = "Nguyễn Hoàng Duy", PhoneNumber = "0931598742", PasswordHash = HashPassword("SecurePassword25!"), ConcurrencyStamp = "55555555-5555-5555-5555-555555555555", Status = AccountStatus.Active, RoleId = 5 }
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
            modelBuilder.Entity<ProductCategories>().HasData(
                new ProductCategories { CategoriesId = 201, ProductId = 1 },
                new ProductCategories { CategoriesId = 201, ProductId = 2 },
                new ProductCategories { CategoriesId = 201, ProductId = 3 },
                new ProductCategories { CategoriesId = 207, ProductId = 4 },
                new ProductCategories { CategoriesId = 201, ProductId = 5 }
            );
            // 4. Seed Warehouses
            modelBuilder.Entity<Warehouse>().HasData(
                new Warehouse { WarehouseId = 1, WarehouseCode = "KVN-01", WarehouseName = "Kho Việt Nam", Address = "Số 10 Đường Cộng Hòa, Phường 13, Quận Tân Bình, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "K20250409-001" },
                new Warehouse { WarehouseId = 2, WarehouseCode = "KHUY-01", WarehouseName = "Kho Hủy", Address = "Khu vực xử lý hàng lỗi, Đường Số 7, KCN Vĩnh Lộc, Bình Chánh, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "KH20250409-002" },
                new Warehouse { WarehouseId = 3, WarehouseCode = "KTHU-01", WarehouseName = "Kho Thuốc", Address = "Số 3B Đường Nguyễn Văn Quá, Đông Hưng Thuận, Quận 12, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "KT20250409-003" },
                new Warehouse { WarehouseId = 4, WarehouseCode = "KMP-01", WarehouseName = "Kho Mỹ Phẩm", Address = "Số 1 Lê Duẩn, Bến Nghé, Quận 1, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "KMP20250409-004" },
                new Warehouse { WarehouseId = 5, WarehouseCode = "KTH-01", WarehouseName = "Kho Trung Hạnh", Address = "Số 88 Đường 3 Tháng 2, Phường 11, Quận 10, TP.HCM", Status = WarehouseStatus.Active, DocumentNumber = "KTH20250409-005" }
            );
            // 5. Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, ProductName = "Paracetamol Stella 500mg", ProductCode = "STP001", SKU = "Viên nén", MadeFrom = "Hóa dược" },
                new Product { ProductId = 2, ProductName = "Aspirin PH8 500mg", ProductCode = "ASP002", SKU = "Viên sủi", MadeFrom = "Hóa dược" },
                new Product { ProductId = 3, ProductName = "Ibuprofen STADA 400mg", ProductCode = "IBU003", SKU = "Viên nang mềm", MadeFrom = "Hóa dược" },
                new Product { ProductId = 4, ProductName = "Vitamin C 500mg (Traphaco)", ProductCode = "VITC004", SKU = "Viên nén sủi", MadeFrom = "Tổng hợp" },
                new Product { ProductId = 5, ProductName = "Thuốc ho Prospan Forte", ProductCode = "TSP005", SKU = "Siro", MadeFrom = "Thảo dược" },
                new Product { ProductId = 6, ProductName = "Amoxicillin 500mg (Pymepharco)", ProductCode = "AMO006", SKU = "Viên nang", MadeFrom = "Bán tổng hợp" },
                new Product { ProductId = 7, ProductName = "Cetirizine Stella 10mg", ProductCode = "CET007", SKU = "Viên nén bao phim", MadeFrom = "Hóa dược" },
                new Product { ProductId = 8, ProductName = "Men vi sinh Biolac Extra", ProductCode = "BIO008", SKU = "Gói bột", MadeFrom = "Vi sinh" },
                new Product { ProductId = 9, ProductName = "Kem dưỡng da BENEW Snail Repair Cream", ProductCode = "SKB009", SKU = "Kem", MadeFrom = "Thiên nhiên" },
                new Product { ProductId = 10, ProductName = "Trà Atiso túi lọc (Ladophar)", ProductCode = "TEA010", SKU = "Túi lọc", MadeFrom = "Thảo dược" },
                new Product { ProductId = 11, ProductName = "Panadol Extra", ProductCode = "PAN011", SKU = "Viên nén sủi", MadeFrom = "Hóa dược" },
                new Product { ProductId = 12, ProductName = "Efferalgan 500mg", ProductCode = "EFF012", SKU = "Viên sủi", MadeFrom = "Hóa dược" },
                new Product { ProductId = 13, ProductName = "Nurofen 200mg", ProductCode = "NUR013", SKU = "Viên nén bao phim", MadeFrom = "Hóa dược" },
                new Product { ProductId = 14, ProductName = "Berocca Performance Cam", ProductCode = "BER014", SKU = "Viên sủi", MadeFrom = "Tổng hợp" },
                new Product { ProductId = 15, ProductName = "Eugica Forte", ProductCode = "EUG015", SKU = "Viên nang mềm", MadeFrom = "Thảo dược" }
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
            modelBuilder.Entity<InboundRequest>().HasData(
                     new InboundRequest { InboundRequestId = 1, InboundRequestCode = "REQ-001", AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 01, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed },
                     new InboundRequest { InboundRequestId = 2, InboundRequestCode = "REQ-002", AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 05, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed },
                     new InboundRequest { InboundRequestId = 3, InboundRequestCode = "REQ-003", AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 10, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed },
                     new InboundRequest { InboundRequestId = 4, InboundRequestCode = "REQ-004", AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 15, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed },
                     new InboundRequest { InboundRequestId = 5, InboundRequestCode = "REQ-005", AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 20, 0, 0, 0, DateTimeKind.Utc)), Status = InboundRequestStatus.Completed }
            );
            modelBuilder.Entity<InboundRequestDetails>().HasData(
                     new InboundRequestDetails { InboundRequestDetailsId = 1, InboundRequestId = 1, ProductId = 1, Quantity = 100, UnitPrice = 25, TotalPrice = 2500 },
                     new InboundRequestDetails { InboundRequestDetailsId = 2, InboundRequestId = 1, ProductId = 2, Quantity = 350, UnitPrice = 30.5M, TotalPrice = 10675 },
                     new InboundRequestDetails { InboundRequestDetailsId = 3, InboundRequestId = 2, ProductId = 3, Quantity = 200, UnitPrice = 35, TotalPrice = 7000 },
                     new InboundRequestDetails { InboundRequestDetailsId = 4, InboundRequestId = 3, ProductId = 4, Quantity = 400, UnitPrice = 25.5M, TotalPrice = 10200 },
                     new InboundRequestDetails { InboundRequestDetailsId = 5, InboundRequestId = 4, ProductId = 5, Quantity = 250, UnitPrice = 47.5M, TotalPrice = 11875 }
            );
            modelBuilder.Entity<Inbound>().HasData(
                    new Inbound { InboundId = 1, InboundCode = "INB-001", InboundRequestId = 1, WarehouseId = 1, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 02, 0, 0, 0, DateTimeKind.Utc)),AccountId = Guid.Parse("44444444-4444-4444-4444-444444444443"), Status = InboundStatus.Completed , ProviderId = 1},
                    new Inbound { InboundId = 2, InboundCode = "INB-002", InboundRequestId = 2, WarehouseId = 2, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 06, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("44444444-4444-4444-4444-444444444443"), Status = InboundStatus.Completed, ProviderId = 2 },
                    new Inbound { InboundId = 3, InboundCode = "INB-003", InboundRequestId = 3, WarehouseId = 1, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 11, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("44444444-4444-4444-4444-444444444443"), Status = InboundStatus.Completed, ProviderId = 3 },
                    new Inbound { InboundId = 4, InboundCode = "INB-004", InboundRequestId = 4, WarehouseId = 3, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 16, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("44444444-4444-4444-4444-444444444443"), Status = InboundStatus.Completed , ProviderId = 4 },
                    new Inbound { InboundId = 5, InboundCode = "INB-005", InboundRequestId = 5, WarehouseId = 2, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 21, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("44444444-4444-4444-4444-444444444443"), Status = InboundStatus.Completed , ProviderId = 5 },
                    new Inbound { InboundId = 6, InboundCode = "INB-006", WarehouseId = 1, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 26, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("44444444-4444-4444-4444-444444444443"), Status = InboundStatus.Completed, ProviderId =1 },
                    new Inbound { InboundId = 10, InboundCode = "INB-P9", WarehouseId = 1, InboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 10, 0, 0, 0, DateTimeKind.Utc)), AccountId = Guid.Parse("44444444-4444-4444-4444-444444444443"), Status = InboundStatus.Completed, ProviderId = 1, InboundRequestId = null }                
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
                new Outbound { OutboundId = 1, OutboundCode = "OUT-001", CustomerId = 1, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 03, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441") ,Note = "abcxyz"},
                new Outbound { OutboundId = 2, OutboundCode = "OUT-002", CustomerId = 2, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 07, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed , AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441") },
                new Outbound { OutboundId = 3, OutboundCode = "OUT-003", CustomerId = 3, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 12, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441") },
                new Outbound { OutboundId = 4, OutboundCode = "OUT-004", CustomerId = 4, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 17, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441") },
                new Outbound { OutboundId = 5, OutboundCode = "OUT-005", CustomerId = 5, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 22, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed , AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441") },
                new Outbound { OutboundId = 8, OutboundCode = "OUT-P9-SELL", CustomerId = 3, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 12, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Completed, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441") },
                new Outbound { OutboundId = 9, OutboundCode = "OUT-P9-RETURN", CustomerId = 3, OutboundDate = Instant.FromDateTimeUtc(new DateTime(2025, 03, 13, 0, 0, 0, DateTimeKind.Utc)), Status = OutboundStatus.Returned, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444441") }
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
                new LotTransfer { LotTransferId = 1, LotTransferCode = "LT-001", FromWareHouseId = 1, ToWareHouseId = 2, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444442"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 03, 0, 0, 0, DateTimeKind.Utc)) },
                new LotTransfer { LotTransferId = 2, LotTransferCode = "LT-002", FromWareHouseId = 1, ToWareHouseId = 3, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444442"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 07, 0, 0, 0, DateTimeKind.Utc)) },
                new LotTransfer { LotTransferId = 3, LotTransferCode = "LT-003", FromWareHouseId = 2, ToWareHouseId = 1, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444442"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 12, 0, 0, 0, DateTimeKind.Utc)) },
                new LotTransfer { LotTransferId = 4, LotTransferCode = "LT-004", FromWareHouseId = 3, ToWareHouseId = 1, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444442"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 17, 0, 0, 0, DateTimeKind.Utc)) },
                new LotTransfer { LotTransferId = 6, LotTransferCode = "LT-P10-IN", FromWareHouseId = 2, ToWareHouseId = 1, AccountId = Guid.Parse("44444444-4444-4444-4444-444444444442"), CreatedAt = Instant.FromDateTimeUtc(new DateTime(2025, 03, 08, 0, 0, 0, DateTimeKind.Utc)), LotTransferStatus = LotTransferStatus.Completed }
                
                
                
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
