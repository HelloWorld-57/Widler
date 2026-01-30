namespace PostsService.Domain.Events.v1
{
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
