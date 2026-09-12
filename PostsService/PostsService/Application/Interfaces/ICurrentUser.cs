namespace PostsService.Application.Interfaces
{
    public interface ICurrentUser
    {
        Guid Id { get; }
        string Username { get; }
        string Email { get; }
        IReadOnlyCollection<string> Roles { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
