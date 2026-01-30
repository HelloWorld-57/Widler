namespace UsersService.Infrastructure.Messaging.Kafka.Events.Users.v1
{
    public record UserUpdatedV1
    {
        public string EventId { get; init; } = Guid.NewGuid().ToString();
        public string UserId { get; init; } = null!;
        public DateTime OccurredAt { get; init; }
        public int Version => 1;
    }
}
