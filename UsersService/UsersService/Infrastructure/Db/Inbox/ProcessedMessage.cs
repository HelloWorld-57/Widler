
namespace UsersService.Infrastructure.Db.Inbox
{
    public class ProcessedMessage
    {
        public string Id { get; set; } = null!;
        public DateTime ProcessedAt { get; set; }
        public string TopicName { get; set; } = null!;
        public int Partition {  get; set; }
        public long Offset { get; set; }

    }
}
