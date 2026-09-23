using PostsService.Domain.Entities;

namespace PostsService.Application.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync(CancellationToken ct = default);
        Task<Post?> GetByIdAsync(string id, CancellationToken ct = default);
        Task AddAsync(Post post);
        //Task UpdateAsync(Post post);
        Task<List<Post>> GetByAuthorIdAsync(string userId, CancellationToken ct);
        Task SaveChangesAsync();
        Task SaveChangesAsync(CancellationToken ct);
    }
}