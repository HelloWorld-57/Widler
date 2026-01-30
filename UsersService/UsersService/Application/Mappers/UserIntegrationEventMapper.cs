using UsersService.Domain.Events.v1;
using UsersService.Infrastructure.Events.Kafka.Messages.Users.v1;
using UsersService.Infrastructure.Messaging.Kafka.Events.Users.v1;

namespace UsersService.Application.Mappers
{
    public static class UserIntegrationEventMapper
    {
        public static UserCreatedV1 ToIntegrationEvent(UserCreatedDomainEvent evt)
            => new() 
            {
                UserId = evt.UserId,
                OccurredAt = evt.OccurredAt
            };

        public static UserUpdatedV1 ToIntegrationEvent(UserUpdatedDomainEvent evt)
            => new()
            {
                UserId = evt.UserId,
                OccurredAt = evt.OccurredAt
            };

        public static UserDeletedV1 ToIntegrationEvent(UserDeletedDomainEvent evt)
            => new()
            {
                UserId = evt.UserId,
                OccurredAt = evt.OccurredAt
            };
    }
}
