using ITAssetManagement1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<InvestmentRequest>()
                .HasMany(ir => ir.Items)
                .WithOne(ii => ii.InvestmentRequest)
                .HasForeignKey(ii => ii.InvestmentRequestId);

            modelBuilder.Entity<Profile>()
                .HasMany(p => p.InvestmentRequests)
                .WithOne(ir => ir.CreatedBy)
                .HasForeignKey(ir => ir.CreatedById);

            modelBuilder.Entity<Profile>()
                .HasMany(p => p.Notifications)
                .WithOne(n => n.Profile)
                .HasForeignKey(n => n.ProfileId);

            // Configure the relationship between Profile and IdentityUser
            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Profile>(p => p.UserId);
        }
    }
}