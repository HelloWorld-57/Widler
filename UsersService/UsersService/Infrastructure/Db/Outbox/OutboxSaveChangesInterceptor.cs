using Microsoft.EntityFrameworkCore.Diagnostics;
using UsersService.Application.Mappers;
using UsersService.Domain.Common;
using UsersService.Domain.Events.v1;
using UsersService.Infrastructure.Messaging.Kafka.Events;
using UsersService.Infrastructure.Messaging.Kafka.Serialization;
using System.Text.Json;

namespace UsersService.Infrastructure.Db.Outbox
{
    public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null)
            {
                return result;
            }

            var domainEvents = context.ChangeTracker
                .Entries<IHasDomainEvents>()
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            if (!domainEvents.Any())
            { 
                return result; 
            }

            var outboxMessages = domainEvents.Select(evt =>
            {
                return evt switch
                {
                    UserCreatedDomainEvent e => CreateOutbox(
                        IntegrationEventNames.Users.CreatedV1,
                        UserIntegrationEventMapper.ToIntegrationEvent(e)
                    ),

                    UserUpdatedDomainEvent e => CreateOutbox(
                        IntegrationEventNames.Users.UpdatedV1,
                        UserIntegrationEventMapper.ToIntegrationEvent(e)
                    ),

                    UserDeletedDomainEvent e => CreateOutbox(
                        IntegrationEventNames.Users.DeletedV1,
                        UserIntegrationEventMapper.ToIntegrationEvent(e)
                    ),

                    _ => throw new InvalidOperationException()
                };
            });

            context.Set<OutboxMessage>().AddRange(outboxMessages);

            foreach (var entity in context.ChangeTracker.Entries<IHasDomainEvents>())
            {
                entity.Entity.ClearDomainEvents();
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        static OutboxMessage CreateOutbox(string type, object payload)
            => new()
            {
                Id = Guid.NewGuid(),
                EventType = type,
                Payload = JsonSerializer.Serialize(payload, JsonOptions.Default),
                OccurredAt = DateTime.UtcNow
            };
    }
}
