using Microsoft.EntityFrameworkCore;
using PostsService.Application.Interfaces;
using PostsService.Domain.Entities;

namespace PostsService.Infrastructure.Db
{
    public class PostRepository : IPostRepository
    {
        private readonly PostsDbContext _db;

        public PostRepository(PostsDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Post>> GetAllAsync(CancellationToken ct = default)
            => await _db.Posts.ToListAsync(ct);

        public async Task<Post?> GetByIdAsync(string id, CancellationToken ct = default)
            => await _db.Posts.FindAsync(id, ct);

        public async Task AddAsync(Post post)
        {
            _db.Posts.Add(post);
        }

        //public async Task UpdateAsync(Post post)
        //{
        //}

        public Task<List<Post>> GetByAuthorIdAsync(string userId, CancellationToken ct)
        {
            return _db.Posts
                .Where(p => p.UserId == userId)
                .ToListAsync(ct);
        }

        public Task SaveChangesAsync()
            => _db.SaveChangesAsync();

        public Task SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
