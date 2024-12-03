using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var seedData = new SeedData(modelBuilder);
            seedData.Seed();
            //modelBuilder.Entity<Account>()
            //    .Property(e => e.AccountId)
            //    .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Account>()
                .Property(e => e.PhoneNumber)
                .HasMaxLength(15);
            
            modelBuilder.Entity<Account>()
                .HasIndex(e => e.Username, "IX_Accounts_Username")
                .IsUnique();

            modelBuilder.Entity<Account>()
                .HasIndex(e => e.Email, "IX_Accounts_Email")
                .IsUnique();

            
            modelBuilder.Entity<Outbound>().HasIndex(e=> e.OutboundCode, "IX_Outbounds_OutboundCode").IsUnique();

            modelBuilder.Entity<Inbound>().HasIndex(e => e.InboundCode, "IX_Inbounds_InboundCode").IsUnique();

            modelBuilder.Entity<Account>()
                .HasIndex(e => e.tOTPSecretKey, "IX_Accounts_TOTPSecretKey")
                .IsDescending()
                .IsUnique();

            modelBuilder.Entity<Drug>()
                .HasIndex(e => e.Code, "IX_Drugs_Code")
                .IsUnique();

            modelBuilder.Entity<Drug>()
                .HasIndex(e => e.SKU, "IX_Drugs_SKU")
                .IsUnique();

        }

        
        public DbSet<Inbound> Inbounds { get; set; }
        public DbSet<Outbound> Outbounds { get; set; }
        public DbSet<InboundDetail> InboundDetails { get; set; }
        public DbSet<OutboundDetail> OutboundDetails { get; set; }  
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Drug> Drug { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Categories> Categories { get; set; }

    }
}
