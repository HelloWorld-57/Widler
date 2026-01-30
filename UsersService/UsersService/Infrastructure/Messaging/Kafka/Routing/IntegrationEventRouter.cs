using UsersService.Application.Messaging;
using UsersService.Infrastructure.Messaging.Kafka.Serialization;
using System.Text.Json;

namespace UsersService.Infrastructure.Messaging.Kafka.Routing
{
    public class IntegrationEventRouter : IIntegrationEventRouter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<IntegrationEventRouter> _logger;

        public IntegrationEventRouter(IServiceScopeFactory scopeFactory, ILogger<IntegrationEventRouter> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task RouteAsync(
            string eventType,
            string payload,
            CancellationToken ct)
        {
            if (!IntegrationEventTypeMap.Map.TryGetValue(eventType, out var clrType))
            {
                _logger.LogDebug("Ignoring event {EventType}", eventType);
                return;
            }
                
            var evt = JsonSerializer.Deserialize(payload, clrType, JsonOptions.Default)!;

            var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(clrType);

            using var scope = _scopeFactory.CreateScope();
            dynamic handler = scope.ServiceProvider.GetRequiredService(handlerType);

            await handler.HandleAsync((dynamic)evt, ct);
        }
    }
}
