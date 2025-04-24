using ITAssetManagement1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ITAssetManagement1.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<InvestmentRequest> InvestmentRequests { get; set; }
        public DbSet<InvestmentItem> InvestmentItems { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the primary key for InvestmentItem (base class)
            modelBuilder.Entity<InvestmentItem>()
                .HasKey(i => i.Id);

            // Make Description nullable
            modelBuilder.Entity<InvestmentItem>()
                .Property(i => i.Description)
                .IsRequired(false); // Allow null values

            

            // Configure decimal precision for currency fields
            modelBuilder.Entity<InvestmentRequest>()
                .Property(r => r.Shipping)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<InvestmentItem>()
                .Property(i => i.UnitCost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<InvestmentItem>()
                .Property(i => i.Shipping)
                .HasColumnType("decimal(18,2)");
        }
    }
}