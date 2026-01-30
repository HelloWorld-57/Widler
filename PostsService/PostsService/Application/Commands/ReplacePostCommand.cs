namespace PostsService.Application.Commands
{
    public record ReplacePostCommand(
        string PostId,
        string Caption,
        string Content);
}
