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
        public DbSet<Ticket> Tickets => Set<Ticket>();
    }
}