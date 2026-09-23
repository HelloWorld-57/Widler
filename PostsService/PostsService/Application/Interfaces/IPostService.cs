using PostsService.Application.Commands;
using PostsService.DTOs.Response;

namespace PostsService.Application.Interfaces
{
    public interface IPostService
    {
        Task<IReadOnlyCollection<PostResponse>> GetAllAsync(CancellationToken ct);
        Task<PostResponse> GetByIdAsync(string id, CancellationToken ct);
        Task<string> CreateAsync(CreatePostCommand command, CancellationToken ct);
        Task UpdateAsync(UpdatePostCommand command, CancellationToken ct);
        Task ReplaceAsync(ReplacePostCommand command, CancellationToken ct);
        Task DeleteAsync(string id, CancellationToken ct);
    }
}
