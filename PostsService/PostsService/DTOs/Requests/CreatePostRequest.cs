namespace PostsService.DTOs.Requests
{
    public record CreatePostRequest(
        string Caption,
        string Content
    );
}
