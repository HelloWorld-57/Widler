namespace PostsService.Infrastructure.Exceptions
{
    public class KafkaPublishException : Exception
    {
        public string Topic { get; }
        public string Key { get; }
        public string Value { get; }

        public KafkaPublishException(string topic, string key, string value, Exception inner)
            : base($"Failed to publish message to Kafka topic '{topic}'", inner)
        {
            Topic = topic;
            Key = key;
            Value = value;
        }
    }
}
