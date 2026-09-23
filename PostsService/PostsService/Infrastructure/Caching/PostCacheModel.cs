namespace PostsService.Infrastructure.Caching
{
    public sealed record PostCacheModel(
        string Id,
        string? Caption,
        string? Content,
        string? UserId,
        DateTime CreationDate,
        bool IsDeleted,
        DateTime? DeletedAt);
}
