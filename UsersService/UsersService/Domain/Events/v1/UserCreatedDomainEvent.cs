namespace UsersService.Domain.Events.v1
{
    public sealed class UserCreatedDomainEvent : IDomainEvent
    {
        public string UserId { get; }
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public UserCreatedDomainEvent(string userId)
        {
            UserId = userId;
        }
    }
}
