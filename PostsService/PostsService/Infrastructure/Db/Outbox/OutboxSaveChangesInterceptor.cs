using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PostsService.Application.Interfaces;
using PostsService.Application.Mappers;
using PostsService.Domain.Common;
using PostsService.Domain.Events.v1;
using PostsService.Infrastructure.Messaging.Kafka.Events;
using PostsService.Infrastructure.Messaging.Kafka.Serialization;
using PostsService.Infrastructure.Observability;
using System.Diagnostics;
using System.Text.Json;

namespace PostsService.Infrastructure.Db.Outbox
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
                    PostCreatedDomainEvent e => CreateOutbox(
                        IntegrationEventNames.Posts.CreatedV1,
                        PostIntegrationEventMapper.ToIntegrationEvent(e)
                    ),

                    PostUpdatedDomainEvent e => CreateOutbox(
                        IntegrationEventNames.Posts.UpdatedV1,
                        PostIntegrationEventMapper.ToIntegrationEvent(e)
                    ),

                    PostDeletedDomainEvent e => CreateOutbox(
                        IntegrationEventNames.Posts.DeletedV1,
                        PostIntegrationEventMapper.ToIntegrationEvent(e)
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
            var correlationId = Activity.Current?.GetTagItem("correlation.id")?.ToString();

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
