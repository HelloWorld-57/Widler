namespace PostsService.Domain.Events.v1
{
    public sealed class PostUpdatedDomainEvent : IDomainEvent
    {
        public string PostId { get; }
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public PostUpdatedDomainEvent(string postId)
        {
            PostId = postId;
        }
    }
}
