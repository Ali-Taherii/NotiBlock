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
        public DbSet<ResellerTicketReadableView> ResellerTicketsReadable => Set<ResellerTicketReadableView>();
        public DbSet<RegulatorReview> RegulatorReviews => Set<RegulatorReview>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== VIEWS ==========
            
            modelBuilder.Entity<ResellerTicketReadableView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_resellertickets_readable");
            });

            // ========== GLOBAL QUERY FILTERS (SOFT DELETE) ==========
            
            modelBuilder.Entity<Product>()
                .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<ConsumerReport>()
                .HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<ResellerTicket>()
                .HasQueryFilter(t => !t.IsDeleted);

            modelBuilder.Entity<Consumer>()
                .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Reseller>()
                .HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<Manufacturer>()
                .HasQueryFilter(m => !m.IsDeleted);

            modelBuilder.Entity<Regulator>()
                .HasQueryFilter(r => !r.IsDeleted);

            // ========== PRODUCT CONFIGURATION ==========
            
            // Unique: Product Serial Number (only for non-deleted products)
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SerialNumber)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            // One-to-many: Manufacturer makes many Products
            modelBuilder.Entity<Product>()
                .HasOne<Manufacturer>()
                .WithMany()
                .HasForeignKey(p => p.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Optional: Reseller assigned to Product
            modelBuilder.Entity<Product>()
                .HasOne<Reseller>()
                .WithMany()
                .HasForeignKey(p => p.ResellerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Optional: Consumer owns Product
            modelBuilder.Entity<Product>()
                .HasOne<Consumer>()
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== CONSUMER REPORT CONFIGURATION ==========
            
            // Configure relationship: ConsumerReport -> Product (by SerialNumber)
            modelBuilder.Entity<ConsumerReport>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.SerialNumber)
                .HasPrincipalKey(p => p.SerialNumber)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: one open report per consumer per product serial number
            modelBuilder.Entity<ConsumerReport>()
                .HasIndex(r => new { r.ConsumerId, r.SerialNumber })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false AND \"Status\" = 0"); // Only for non-deleted, pending reports

            // One-to-many: ConsumerReport linked to ResellerTicket (when escalated)
            modelBuilder.Entity<ConsumerReport>()
                .HasOne(r => r.ResellerTicket)
                .WithMany(t => t.ConsumerReports)
                .HasForeignKey(r => r.ResellerTicketId)
                .OnDelete(DeleteBehavior.SetNull);

            // ========== RESELLER TICKET CONFIGURATION ==========
            
            // Unique constraint: Resellers cannot have multiple open tickets with the same category
            modelBuilder.Entity<ResellerTicket>()
                .HasIndex(t => new { t.ResellerId, t.Category })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false AND \"Status\" = 0"); // Only for non-deleted, pending tickets

            // ========== RECALL CONFIGURATION ==========
            modelBuilder.Entity<Recall>(entity =>
            {
                // Index on ProductId for faster lookups
                entity.HasIndex(r => r.ProductSerialNumber);

                // Index on Status for filtering
                entity.HasIndex(r => r.Status);

                // Composite index for common query pattern
                entity.HasIndex(r => new { r.ProductSerialNumber, r.Status, r.IsDeleted });

                // Foreign key to Manufacturer
                entity.HasOne(r => r.Manufacturer)
                      .WithMany()
                      .HasForeignKey(r => r.ManufacturerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Global query filter for soft deletes
            modelBuilder.Entity<Recall>().HasQueryFilter(r => !r.IsDeleted);


            // ========== REGULATOR REVIEW CONFIGURATION ==========
            modelBuilder.Entity<RegulatorReview>()
                .HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<RegulatorReview>()
                .HasOne(r => r.Ticket)
                .WithMany(t => t.RegulatorReviews)
                .HasForeignKey(r => r.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RegulatorReview>()
                .HasOne(r => r.Regulator)
                .WithMany()
                .HasForeignKey(r => r.RegulatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate reviews for same ticket by same regulator
            modelBuilder.Entity<RegulatorReview>()
                .HasIndex(r => new { r.TicketId, r.RegulatorId })
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");


            // ========= NOTIFICATION CONFIGURATION ==========
            modelBuilder.Entity<Notification>()
            .HasQueryFilter(n => !n.IsDeleted);

            // Index for faster queries by recipient
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.RecipientId, n.IsRead, n.CreatedAt });

            // Index for unread notifications
            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.RecipientId, n.IsRead })
                .HasFilter("\"IsRead\" = false");
        }
    }
}