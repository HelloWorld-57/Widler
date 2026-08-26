namespace UsersService.Application.Interfaces
{
    public interface ICurrentUser
    {
        Guid Id { get; }
        string Username { get; }
        IReadOnlyCollection<string> Roles { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
