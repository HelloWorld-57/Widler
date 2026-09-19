namespace UsersService.DTOs.Requests
{
    public record UpdateUserRequest(
        string Id,
        string? Name,
        string? SecondName,
        DateTime? BirthDate
    );
}
