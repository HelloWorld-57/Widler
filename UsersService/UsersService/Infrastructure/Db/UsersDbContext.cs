using Microsoft.EntityFrameworkCore;
using UsersService.Domain.Entities;
using UsersService.Infrastructure.Db.Inbox;
using UsersService.Infrastructure.Db.Outbox;

namespace UsersService.Infrastructure.Db
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasQueryFilter(p => !p.IsDeleted);
        }

        public DbSet<User> Users {  get; set; }
        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    }
}
