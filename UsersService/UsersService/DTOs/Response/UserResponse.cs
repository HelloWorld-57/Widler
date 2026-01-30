namespace UsersService.DTOs.Response
{
    public record UserResponse(
        string Id,
        string Name,
        string SecondName,
        string Email,
        DateTime BirthDate,
        DateTime CreationDate
    );
}
