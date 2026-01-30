using Microsoft.EntityFrameworkCore;

namespace PostsService.Infrastructure.Db.Inbox
{
    public sealed class InboxService : IInboxService
    {
        private readonly PostsDbContext _db;

        public InboxService(PostsDbContext db)
        {
            _db = db;
        }

        public Task<bool> IsProcessedAsync(string messageId, CancellationToken ct)
            => _db.ProcessedMessages.AnyAsync(x => x.Id == messageId, ct);

        public async Task MarkProcessedAsync(
            string messageId,
            string topic,
            int partition,
            long offset,
            CancellationToken ct)
        {
            _db.ProcessedMessages.Add(new ProcessedMessage
            {
                Id = messageId,
                TopicName = topic,
                Partition = partition,
                Offset = offset,
                ProcessedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }

    }
}
