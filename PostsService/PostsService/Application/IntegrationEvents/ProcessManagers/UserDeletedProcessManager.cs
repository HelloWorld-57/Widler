using PostsService.Application.Interfaces;
using PostsService.Application.Messaging;
using PostsService.Infrastructure.Events.Kafka.Messages.Users.v1;
using PostsService.Infrastructure.Telemetry;
using System.Diagnostics;

namespace PostsService.Application.IntegrationEvents.ProcessManagers
{
    public sealed class UserDeletedProcessManager : IIntegrationEventHandler<UserDeletedV1>
    {
        private readonly IPostRepository _posts;

        public UserDeletedProcessManager(IPostRepository posts)
        {
            _posts = posts;
        }

        public async Task HandleAsync(UserDeletedV1 evt, CancellationToken ct)
        {
            using var activity = Tracing.ActivitySource.StartActivity("Saga.UserDeleted.CascadePosts", ActivityKind.Internal);

            activity?.SetTag("saga.name", "UserDeletionCascade");
            activity?.SetTag("user.id", evt.UserId);
            activity?.SetTag("event.id", evt.EventId);

            var posts = await _posts.GetByAuthorIdAsync(evt.UserId, ct);

            activity?.SetTag("posts.count", posts.Count);

            foreach (var post in posts)
            {
                post.Delete();
            }

            await _posts.SaveChangesAsync(ct);
        }
    }
}
