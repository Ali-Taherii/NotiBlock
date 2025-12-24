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
        //public DbSet<Ticket> Tickets => Set<Ticket>();

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ConsumerReport> ConsumerReports => Set<ConsumerReport>();
        public DbSet<ResellerTicket> ResellerTickets => Set<ResellerTicket>();

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique: Product Serial Number
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SerialNumber)
                .IsUnique();

            // One-to-many: Consumer owns many Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-many: Manufacturer makes many Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Manufacturer)
                .WithMany()
                .HasForeignKey(p => p.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional Reseller
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Reseller)
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