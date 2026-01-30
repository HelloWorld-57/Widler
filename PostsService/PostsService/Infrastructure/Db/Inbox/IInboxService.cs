namespace PostsService.Infrastructure.Db.Inbox
{
    public interface IInboxService
    {
        Task<bool> IsProcessedAsync(string messageId, CancellationToken ct);
        Task MarkProcessedAsync(
            string messageId,
            string topic,
            int partition,
            long offset,
            CancellationToken ct);
    }
}
