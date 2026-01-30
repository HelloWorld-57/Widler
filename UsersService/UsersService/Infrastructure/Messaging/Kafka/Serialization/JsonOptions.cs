using System.Text.Json;
using System.Text.Json.Serialization;

namespace UsersService.Infrastructure.Messaging.Kafka.Serialization
{
    public static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = Create();

        private static JsonSerializerOptions Create()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };

            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            return options;
        }
    }
}
