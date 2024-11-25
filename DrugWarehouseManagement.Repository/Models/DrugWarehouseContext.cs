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
            modelBuilder.Entity<Account>()
                .Property(e => e.AccountId)
                .HasDefaultValueSql("NEWID()");
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
