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
            modelBuilder.Entity<Account>()
                .Property(e => e.AccountId)
                .HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Product>()
                .Property(e => e.ProductId)
                .HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Drug>()
                .Property(e => e.DrugId)
                .HasDefaultValueSql("NEWID()");

            modelBuilder.Entity<Account>()
                .Property(e => e.PhoneNumber)
                .HasMaxLength(15);
            
            modelBuilder.Entity<Account>()
                .HasIndex(e => e.Username, "IX_Accounts_Username")
                .IsUnique();

            modelBuilder.Entity<Account>()
                .HasIndex(e => e.Email, "IX_Accounts_Email")
                .IsUnique();
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Drug> Drugs { get; set; }
    }
}
