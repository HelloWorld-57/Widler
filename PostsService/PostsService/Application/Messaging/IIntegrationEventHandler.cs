namespace PostsService.Application.Messaging
{
    public interface IIntegrationEventHandler<in TEvent>
    {
        Task HandleAsync(TEvent evt, CancellationToken ct);
    }
}
