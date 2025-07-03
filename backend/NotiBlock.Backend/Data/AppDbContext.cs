using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Recall> Recalls => Set<Recall>();
    }
}