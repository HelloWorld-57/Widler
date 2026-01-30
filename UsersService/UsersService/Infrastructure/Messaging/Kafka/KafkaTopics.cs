namespace UsersService.Infrastructure.Messaging.Kafka
{
    public class KafkaTopics
    {
        public string Users { get; init; } = null!;
        public string UsersDlq { get; init; } = null!;

        public string Posts { get; init; } = null!;
        public string PostsDlq { get; init; } = null!;
    }
}
