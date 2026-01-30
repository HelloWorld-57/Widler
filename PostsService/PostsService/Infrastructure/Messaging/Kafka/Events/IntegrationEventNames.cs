namespace PostsService.Infrastructure.Messaging.Kafka.Events
{
    public static class IntegrationEventNames
    {
        public static class Posts
        {
            public const string CreatedV1 = "Posts.Created.V1";
            public const string UpdatedV1 = "Posts.Updated.V1";
            public const string DeletedV1 = "Posts.Deleted.V1";
        }

        public static class Users
        {
            public const string CreatedV1 = "Users.Created.V1";
            public const string UpdatedV1 = "Users.Updated.V1";
            public const string DeletedV1 = "Users.Deleted.V1";
        }

        public static class Dlq
        {
            public const string MessageV1 = "DLQ.Message.V1";
        }
    }
}
