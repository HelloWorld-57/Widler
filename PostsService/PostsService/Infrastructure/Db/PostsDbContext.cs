using Microsoft.EntityFrameworkCore;
using PostsService.Domain.Entities;
using PostsService.Infrastructure.Db.Inbox;
using PostsService.Infrastructure.Db.Outbox;

namespace PostsService.Infrastructure.Db
{
    public class PostsDbContext : DbContext
    {
        public PostsDbContext(DbContextOptions<PostsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>().HasQueryFilter(p => !p.IsDeleted);
        }

        public DbSet<Post> Posts {  get; set; }
        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    }
}
