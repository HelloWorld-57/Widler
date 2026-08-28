using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Diagnostics;
using System.Text.Json;
using UsersService.Application.Interfaces;
using UsersService.Application.Mappers;
using UsersService.Domain.Common;
using UsersService.Domain.Events.v1;
using UsersService.Infrastructure.Messaging.Kafka.Events;
using UsersService.Infrastructure.Messaging.Kafka.Serialization;

namespace UsersService.Infrastructure.Db.Outbox
{
    public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICorrelationContext _correlationContext;

        public OutboxSaveChangesInterceptor(ICorrelationContext correlationContext)
        {
            _correlationContext = correlationContext;
        }

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

        private OutboxMessage CreateOutbox(string type, object payload)
        {
            var activity = Activity.Current;

            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = type,
                Payload = JsonSerializer.Serialize(payload, JsonOptions.Default),
                OccurredAt = DateTime.UtcNow,
                TraceParent = activity?.Id,
                TraceState = activity?.TraceStateString,
                CorrelationId = _correlationContext.CorrelationId
            };
        }
    }
}
