using PostsService.Domain.Entities;

namespace PostsService.Application.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(string id);
        Task AddAsync(Post post);
        //Task UpdateAsync(Post post);
        Task<List<Post>> GetByAuthorIdAsync(string userId, CancellationToken ct);
        Task SaveChangesAsync();
        Task SaveChangesAsync(CancellationToken ct);
    }
}