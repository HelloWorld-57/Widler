using PostsService.Domain.Entities;
using static PostsService.Infrastructure.Messaging.Kafka.Events.IntegrationEventNames;

namespace PostsService.Application.Interfaces
{
    public interface IPostCache
    {
        Task<Post?> GetAsync(string postId, CancellationToken cancellationToken = default);

        Task SetAsync(Post post, CancellationToken cancellationToken = default);

        Task RemoveAsync(string postId, CancellationToken cancellationToken = default);
    }
}
