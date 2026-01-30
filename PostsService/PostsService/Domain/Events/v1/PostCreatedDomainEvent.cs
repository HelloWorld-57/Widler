namespace PostsService.Domain.Events.v1
{
    public sealed class PostCreatedDomainEvent : IDomainEvent
    {
        public string PostId { get; }
        public string UserId { get; }
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public PostCreatedDomainEvent(string postId, string userId)
        {
            PostId = postId;
            UserId = userId;
        }
    }
}
