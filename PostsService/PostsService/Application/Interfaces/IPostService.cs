using PostsService.Application.Commands;
using PostsService.DTOs.Response;

namespace PostsService.Application.Interfaces
{
    public interface IPostService
    {
        Task<IReadOnlyCollection<PostResponse>> GetAllAsync();
        Task<PostResponse> GetByIdAsync(string id);
        Task<string> CreateAsync(CreatePostCommand command);
        Task UpdateAsync(UpdatePostCommand command);
        Task ReplaceAsync(ReplacePostCommand command);
        Task DeleteAsync(string id);
    }
}
