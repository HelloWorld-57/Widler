namespace PostsService.Application.Commands
{
    public record UpdatePostCommand(
        string PostId,
        string? Caption,
        string? Content
    );
}
