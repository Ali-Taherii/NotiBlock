using Microsoft.EntityFrameworkCore;
using NotiBlock.Backend.Models;

namespace NotiBlock.Backend.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Recall> Recalls => Set<Recall>();
        public DbSet<AppUser> AppUsers => Set<AppUser>();

        public DbSet<Consumer> Consumers => Set<Consumer>();
        public DbSet<ConsumerResponse> ConsumerResponses => Set<ConsumerResponse>();
    }
} 