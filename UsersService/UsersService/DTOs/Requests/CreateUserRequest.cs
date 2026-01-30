namespace UsersService.DTOs.Requests
{
    public record CreateUserRequest(
        string Name,
        string SecondName,
        string Email,
        DateTime BirthDate
    );
}
