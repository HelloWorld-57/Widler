namespace PostsService.DTOs.Requests
{
    public record UpdatePostRequest(
        string? Caption,
        string? Content
    );
}
