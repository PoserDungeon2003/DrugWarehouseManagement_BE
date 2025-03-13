using Microsoft.EntityFrameworkCore;

namespace DrugWarehouseManagement.Repository.Models
{
    public class DrugWarehouseContext : DbContext
    {
        public DrugWarehouseContext()
        {
        }

        public DrugWarehouseContext(DbContextOptions<DrugWarehouseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var seedData = new SeedData(modelBuilder);
            seedData.Seed();
            //modelBuilder.Entity<Account>()
            //    .Property(e => e.AccountId)
            //    .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Account>(entity =>
            {
                entity
                .Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

                entity.Property(e => e.AccountSettings)
                .HasColumnType("jsonb");

                entity.HasIndex(e => e.PhoneNumber, "IX_Accounts_PhoneNumber")
                .IsUnique();

                entity.HasIndex(e => e.UserName, "IX_Accounts_UserName")
                .IsUnique();

                entity.HasIndex(e => e.Email, "IX_Accounts_Email")
                .IsUnique();

                entity.HasIndex(e => e.tOTPSecretKey, "IX_Accounts_TOTPSecretKey")
                .IsDescending()
                .IsUnique();
            });
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15);

                entity.HasIndex(e => e.PhoneNumber,"IX_Customers_PhoneNumber")
                .IsUnique();
                entity.HasIndex(e => e.Email, "IX_Customers_Email")
                .IsUnique();
                entity.HasIndex(e => e.DocumentNumber, "IX_Customers_DocumentNumber")
                .IsUnique();
            });
            modelBuilder.Entity<Outbound>(entity =>
            {
                entity.HasIndex(e => e.OutboundCode, "IX_Outbounds_OutboundCode")
                .IsUnique();
            });

            modelBuilder.Entity<Inbound>(entity =>
            {
                entity.HasIndex(e => e.InboundCode, "IX_Inbounds_InboundCode").IsUnique();
                entity.HasIndex(e => e.ProviderOrderCode, "IX_Inbounds_ProviderOrderCode").IsUnique();
            });

            modelBuilder.Entity<AuditLogs>(entity =>
            {
                entity.Property(e => e.Payload)
                    .HasColumnType("jsonb");
            });

            modelBuilder.Entity<Lot>(entity =>
            {
                entity.HasIndex(l => new { l.LotNumber, l.ExpiryDate, l.ProviderId, l.WarehouseId }, "IX_Lots_LotNumber_ExpiryDate_ProviderId_WarehouseId")
                    .IsUnique();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.ProductCode, "IX_Products_ProductCode")
                    .IsUnique();
                entity.HasMany(e => e.Categories)
                    .WithMany(e => e.Products)
                    .UsingEntity<ProductCategories>();
            });

            modelBuilder.Entity<InboundDetails>(entity => 
            {
                entity.HasIndex(e => e.LotNumber, "IX_InboundDetails_LotNumber");
            });

            modelBuilder.Entity<LotTransfer>(entity =>
            {
                entity.HasIndex(e => e.LotTransferCode, "IX_LotTransfer_LotTransferCode")
                    .IsUnique();
            });

            modelBuilder.Entity<Provider>(entity =>
            {
                entity.HasIndex(e => e.PhoneNumber, "IX_Provider_PhoneNumber")
                    .IsUnique();
                entity.HasIndex(e => e.DocumentNumber, "IX_Provider_DocumentNumber")
                    .IsUnique();
            });

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasIndex(e => e.WarehouseCode, "IX_Warehouse_WarehouseCode")
                    .IsUnique();
            });

            modelBuilder.Entity<LotTransfer>(entity =>
            {
                entity.HasIndex(e => e.LotTransferCode)
                    .IsUnique();
                entity.HasOne(e => e.FromWareHouse)
                    .WithMany()
                    .HasForeignKey(e => e.FromWareHouseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ToWareHouse)
                    .WithMany()
                    .HasForeignKey(e => e.ToWareHouseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Categories>(entity =>
            {
                entity.HasMany(e => e.Products)
                    .WithMany(e => e.Categories)
                    .UsingEntity<ProductCategories>();
                entity.HasOne(e => e.ParentCategory)
                    .WithMany(e => e.SubCategories)
                    .HasForeignKey(e => e.ParentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasMany(e => e.Providers)
                    .WithMany(e => e.Assets)
                    .UsingEntity<ProviderAssets>();
                entity.HasMany(e => e.Inbounds)
                    .WithMany(e => e.Assets)
                    .UsingEntity<InboundAssets>();
                entity.HasMany(e => e.InboundReports)
                    .WithMany(e => e.Assets)
                    .UsingEntity<InboundReportAssets>();
            });

        }


        public DbSet<Inbound> Inbounds { get; set; }
        public DbSet<Customer> Customers { get; set; }  
        public DbSet<Outbound> Outbounds { get; set; }
        public DbSet<InboundDetails> InboundDetails { get; set; }
        public DbSet<OutboundDetails> OutboundDetails { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<LotTransfer> LotTransfers { get; set; }
        public DbSet<LotTransferDetail> LotTransferDetails { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<InboundReport> InboundReports { get; set; }
        public DbSet<InboundRequest> InboundRequests { get; set; }
        public DbSet<Asset> Assets { get; set; }
    }
}
