namespace UsersService.Infrastructure.Messaging.Kafka.Events.Keycloak
{
    public static class KeycloakAdminEventExtensions
    {
        public static string? GetUserId(this KeycloakAdminEvent adminEvent)
        {
            const string prefix = "users/";

            if (string.IsNullOrWhiteSpace(adminEvent.ResourcePath))
            {
                return null;
            }

            if (!adminEvent.ResourcePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var userId = adminEvent.ResourcePath[prefix.Length..];

            return string.IsNullOrWhiteSpace(userId) ? null : userId;
        }
    }
}
