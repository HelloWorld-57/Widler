namespace UsersService.Infrastructure.Caching
{
    public sealed record UserCacheModel(
        string Id,
        string Email,
        string Username,
        string? Name,
        string? SecondName,
        DateTime? BirthDate,
        DateTime CreationDate,
        bool IsEnabled,
        bool IsDeleted,
        DateTime? DeletedAt);
}
