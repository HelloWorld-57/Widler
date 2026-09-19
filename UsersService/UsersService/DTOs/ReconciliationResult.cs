namespace UsersService.DTOs
{
    public sealed record ReconciliationResult(
        int KeycloakUsers,
        int LocalUsers,
        int Created,
        int Restored,
        int Updated,
        int Deleted);
}
