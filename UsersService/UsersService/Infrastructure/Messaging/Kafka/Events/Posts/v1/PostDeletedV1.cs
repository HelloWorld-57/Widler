namespace UsersService.Infrastructure.Events.Kafka.Messages.Posts.v1
{
    public record PostDeletedV1
    {
        public string EventId { get; init; } = Guid.NewGuid().ToString();
        public string PostId { get; init; } = null!;
        public string UserId { get; init; } = null!;
        public DateTime OccurredAt { get; init; }
        public int Version => 1;
    }
}
