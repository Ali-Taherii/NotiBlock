using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Consumer> Consumers => Set<Consumer>();
        public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
        public DbSet<Reseller> Resellers => Set<Reseller>();
        public DbSet<Regulator> Regulators => Set<Regulator>();
        public DbSet<Recall> Recalls => Set<Recall>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ConsumerReport> ConsumerReports => Set<ConsumerReport>();
        public DbSet<ResellerTicket> ResellerTickets => Set<ResellerTicket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global query filter for soft delete
            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => !p.IsDeleted);

            // Unique: Product Serial Number (only for non-deleted products)
            // PostgreSQL syntax for filtered index
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SerialNumber)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false"); // PostgreSQL uses double quotes for identifiers

            // One-to-many: Consumer owns many Products
            modelBuilder.Entity<Product>()
                .HasOne<Consumer>()
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-many: Manufacturer makes many Products
            modelBuilder.Entity<Product>()
                .HasOne<Manufacturer>()
                .WithMany()
                .HasForeignKey(p => p.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional Reseller
            modelBuilder.Entity<Product>()
                .HasOne<Reseller>()
                .WithMany()
                .HasForeignKey(p => p.ResellerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Unique constraint: one open report per consumer per product
            modelBuilder.Entity<ConsumerReport>()
                .HasIndex(r => new { r.ConsumerId, r.ProductId })
                .IsUnique();

            // One-to-many: Consumer reports linked to ResellerTicket
            modelBuilder.Entity<ConsumerReport>()
                .HasOne(r => r.ResellerTicket)
                .WithMany(t => t.ConsumerReports)
                .HasForeignKey(r => r.ResellerTicketId)
                .OnDelete(DeleteBehavior.SetNull);

            // Resellers cannot have multiple open tickets with the same category
            modelBuilder.Entity<ResellerTicket>()
                .HasIndex(t => new { t.ResellerId, t.Category })
                .IsUnique();
        }
    }
}