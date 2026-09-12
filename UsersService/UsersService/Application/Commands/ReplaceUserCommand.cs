namespace UsersService.Application.Commands
{
    public record ReplaceUserCommand(
        string UserId,
        string Name,
        string SecondName,
        DateTime BirthDate);
}
