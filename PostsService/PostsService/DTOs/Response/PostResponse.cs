namespace PostsService.DTOs.Response
{
    public record PostResponse(
        string Id,
        string Caption,
        string Content,
        string UserId,
        DateTime CreationDate
    );
}
