namespace PostsService.DTOs.Requests
{
    public record ReplacePostRequest(
        string Caption,
        string Content
    );
}
