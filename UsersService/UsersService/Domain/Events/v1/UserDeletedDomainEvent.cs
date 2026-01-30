namespace UsersService.Domain.Events.v1
{
    public sealed class UserDeletedDomainEvent : IDomainEvent
    {
        public string UserId { get; }
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public UserDeletedDomainEvent(string userId)
        {
            UserId = userId;
        }
    }
}
