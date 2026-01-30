namespace UsersService.Infrastructure.Db.Outbox
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = null!; 
        public string Payload { get; set; } = null!;
        public DateTime OccurredAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public int Attempts { get; set; }
    }
}
