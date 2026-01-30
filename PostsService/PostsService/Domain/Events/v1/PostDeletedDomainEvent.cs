namespace PostsService.Domain.Events.v1
{
    public sealed class PostDeletedDomainEvent : IDomainEvent
    {
        public string PostId { get; }
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public PostDeletedDomainEvent(string postId)
        {
            PostId = postId;
        }
    }
}
