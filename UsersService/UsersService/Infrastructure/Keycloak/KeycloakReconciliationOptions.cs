namespace UsersService.Infrastructure.Keycloak
{
    public sealed class KeycloakReconciliationOptions
    {
        public const string SectionName = "Keycloak:Reconciliation";

        public int IntervalMinutes { get; init; } = 15;
    }
}
