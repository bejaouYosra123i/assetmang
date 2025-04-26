namespace ITAssetManagement1.Data
{
    using ITAssetManagement1.Models;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;

    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<InvestmentRequest> InvestmentRequests { get; set; }
        public DbSet<InvestmentItem> InvestmentItems { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ITRequest> ITRequests { get; set; }
        public DbSet<Validation> Validations { get; set; }
        public DbSet<PcRequest> PcRequests { get; set; } // Corrected DbSet for PcRequest

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the primary key for InvestmentItem (base class)
            modelBuilder.Entity<InvestmentItem>()
                .HasKey(i => i.Id);

            // Make Description nullable
            modelBuilder.Entity<InvestmentItem>()
                .Property(i => i.Description)
                .IsRequired(false);

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

            // Configure the relationship between ITRequest and IdentityUser
            modelBuilder.Entity<ITRequest>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey("RequesterId")
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the relationship between ITRequest and Validation
            modelBuilder.Entity<Validation>()
                .HasOne(v => v.ITRequest)
                .WithMany(r => r.Validations)
                .HasForeignKey(v => v.ITRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure PcRequest inheritance (TPH)
            modelBuilder.Entity<ITRequest>()
                .HasDiscriminator<string>("RequestType")
                .HasValue<ITRequest>("ITRequest")
                .HasValue<PcRequest>("PcRequest");
        }
    }
}