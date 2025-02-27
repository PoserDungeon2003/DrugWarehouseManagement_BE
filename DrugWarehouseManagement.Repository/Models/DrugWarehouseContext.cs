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
                entity.HasOne(l => l.TemporaryWarehouse)
                    .WithMany()
                    .HasForeignKey(l => l.TemporaryWarehouseId);

                entity.HasIndex(e => e.LotNumber, "IX_Lots_LotNumber")
                    .IsUnique();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.ProductCode, "IX_Products_ProductCode")
                    .IsUnique();
            });

            modelBuilder.Entity<InboundDetails>(entity => 
            {
                entity.HasIndex(e => e.LotNumber, "IX_InboundDetails_LotNumber");
            });

            modelBuilder.Entity<TransferOrder>(entity =>
            {
                entity.HasIndex(e => e.TransferOrderCode, "IX_TransferOrder_TransferOrderCode")
                    .IsUnique();
            });

            modelBuilder.Entity<Provider>(entity =>
            {
                entity.HasIndex(e => e.PhoneNumber, "IX_Provider_PhoneNumber")
                    .IsUnique();
            });

            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.HasIndex(e => e.WarehouseCode, "IX_Warehouse_WarehouseCode")
                    .IsUnique();
            });

        }


        public DbSet<Inbound> Inbounds { get; set; }
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
        public DbSet<TransferOrder> TransferOrders { get; set; }
        public DbSet<TransferOrderDetail> TransferOrderDetails { get; set; }
    }
}
