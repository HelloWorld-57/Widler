namespace UsersService.Domain.Events.v1
{
    public sealed class UserUpdatedDomainEvent : IDomainEvent
    {
        public string UserId { get; }
        public DateTime OccurredAt { get; } = DateTime.UtcNow;

        public UserUpdatedDomainEvent(string userId)
        {
            UserId = userId;
        }
    }
}
