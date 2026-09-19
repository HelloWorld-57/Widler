using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UsersService.Infrastructure.Db
{
    public sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
    {
        public UsersDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<UsersDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=usersdb;Username=users_user;Password=users_pass")
                .UseSnakeCaseNamingConvention()
                .Options;

            //UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))

            return new UsersDbContext(options);
        }
    }
}
