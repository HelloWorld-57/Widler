using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace PostsService.Infrastructure.Db.Extensions
{
    public static class DbUpdateExceptionExtensions
    {
        public static bool IsUniqueViolation(this DbUpdateException ex)
        {
            return ex.InnerException switch
            {
                PostgresException pg when pg.SqlState == PostgresErrorCodes.UniqueViolation
                    => true,

                _ => false
            };
        }
    }
}
