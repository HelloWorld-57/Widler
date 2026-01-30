using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PostsService.Infrastructure.Db
{
    public sealed class PostsDbContextFactory : IDesignTimeDbContextFactory<PostsDbContext>
    {
        public PostsDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<PostsDbContext>()
                .UseNpgsql("Host=localhost;Port=5434;Database=postsdb;Username=posts_user;Password=posts_pass")
                .Options;

            //UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))

            return new PostsDbContext(options);
        }
    }
}
