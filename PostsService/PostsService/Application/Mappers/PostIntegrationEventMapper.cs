using PostsService.Domain.Events.v1;
using PostsService.Infrastructure.Events.Kafka.Messages.Posts.v1;

namespace PostsService.Application.Mappers
{
    public static class PostIntegrationEventMapper
    {
        public static PostCreatedV1 ToIntegrationEvent(PostCreatedDomainEvent evt)
            => new() 
            {
                PostId = evt.PostId,
                UserId = evt.UserId,
                OccurredAt = evt.OccurredAt
            };

        public static PostUpdatedV1 ToIntegrationEvent(PostUpdatedDomainEvent evt)
            => new()
            {
                PostId = evt.PostId,
                OccurredAt = evt.OccurredAt
            };

        public static PostDeletedV1 ToIntegrationEvent(PostDeletedDomainEvent evt)
            => new()
            {
                PostId = evt.PostId,
                OccurredAt = evt.OccurredAt
            };
    }
}
