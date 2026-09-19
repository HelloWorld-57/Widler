namespace PostsService.Application.Commands
{
    public record CreatePostCommand(
        string Caption,
        string Content
    );
}
