namespace UsersService.Application.Messaging
{
    public interface IIntegrationEventRouter
    {
        Task RouteAsync(string eventType, string payload, CancellationToken ct);
    }
}
