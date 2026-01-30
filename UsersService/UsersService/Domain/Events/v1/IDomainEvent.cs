namespace UsersService.Domain.Events.v1
{
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
