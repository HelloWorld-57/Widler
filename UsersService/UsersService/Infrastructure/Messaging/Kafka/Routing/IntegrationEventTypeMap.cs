using UsersService.Infrastructure.Events.Kafka.Messages.Posts.v1;
using UsersService.Infrastructure.Events.Kafka.Messages.Users.v1;
using UsersService.Infrastructure.Messaging.Kafka.Events;
using UsersService.Infrastructure.Messaging.Kafka.Events.Users.v1;

namespace UsersService.Infrastructure.Messaging.Kafka.Routing
{
    public static class IntegrationEventTypeMap
    {
        public static readonly Dictionary<string, Type> Map = new()
        {
            //[IntegrationEventNames.Posts.CreatedV1] = typeof(PostCreatedV1),
            //[IntegrationEventNames.Posts.UpdatedV1] = typeof(PostUpdatedV1),
            //[IntegrationEventNames.Posts.DeletedV1] = typeof(PostDeletedV1),

            //[IntegrationEventNames.Users.CreatedV1] = typeof(UserCreatedV1),
            //[IntegrationEventNames.Users.UpdatedV1] = typeof(UserUpdatedV1),
            //[IntegrationEventNames.Users.DeletedV1] = typeof(UserDeletedV1),
        };
    }
}
