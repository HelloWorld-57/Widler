namespace UsersService.Infrastructure.Messaging.Kafka.Dlq
{
    public record DlqMessage(
        string OriginalTopic,
        int Partition,
        long Offset,
        string? Key,
        string Payload,
        string Error,
        DateTime FailedAt,
        string? TraceId
    );
}
