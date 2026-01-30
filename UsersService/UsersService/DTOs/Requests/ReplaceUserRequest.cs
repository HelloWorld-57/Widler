namespace UsersService.DTOs.Requests
{
    public record ReplaceUserRequest(
        string Id,
        string Name,
        string SecondName,
        string Email,
        DateTime BirthDate
    );
}
