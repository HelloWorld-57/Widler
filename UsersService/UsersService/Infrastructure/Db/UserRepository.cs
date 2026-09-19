using Microsoft.EntityFrameworkCore;
using UsersService.Application.Interfaces;
using UsersService.Domain.Entities;

namespace UsersService.Infrastructure.Db
{
    public class UserRepository : IUserRepository
    {
        private readonly UsersDbContext _db;

        public UserRepository(UsersDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _db.Users.ToListAsync();

        public async Task<IReadOnlyCollection<User>> GetAllIncludingDeletedAsync(CancellationToken ct = default)
        {
            return await _db.Users
                .IgnoreQueryFilters()
                .ToListAsync(ct);
        }

        public async Task<User?> GetByIdAsync(string id)
            => await _db.Users.FindAsync(id);

        public async Task<User?> GetByIdIncludingDeletedAsync(string id, CancellationToken ct = default)
        {
            return await _db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    user => user.Id == id,
                    ct);
        }

        public async Task AddAsync(User user)
        {
            _db.Users.Add(user);
        }

        //public async Task UpdateAsync(User user)
        //{
        //}

        public Task SaveChangesAsync()
            => _db.SaveChangesAsync();

        public Task SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
