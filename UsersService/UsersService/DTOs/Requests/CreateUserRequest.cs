namespace UsersService.DTOs.Requests
{
    public record CreateUserRequest(
        string Id,
        string Username,
        string Email
    );
}
