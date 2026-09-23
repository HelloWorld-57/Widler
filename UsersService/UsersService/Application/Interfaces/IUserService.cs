using UsersService.Application.Commands;
using UsersService.DTOs.Response;

namespace UsersService.Application.Interfaces
{
    public interface IUserService
    {
        Task<IReadOnlyCollection<UserResponse>> GetAllAsync(CancellationToken ct);
        Task<UserResponse> GetByIdAsync(string id, CancellationToken ct);
        //Task<string> CreateAsync(CreateUserCommand command);
        Task UpdateAsync(UpdateUserCommand command, CancellationToken ct);
        Task ReplaceAsync(ReplaceUserCommand command, CancellationToken ct);
        Task DeleteAsync(string id, CancellationToken ct);

        Task CreateFromKeycloakAsync(CreateUserFromKeycloakCommand cmd, CancellationToken ct);
        Task UpdateEmailFromKeycloakAsync(UpdateUserEmailFromKeycloakCommand cmd, CancellationToken ct);
        Task DeleteFromKeycloakAsync(string userId, CancellationToken ct);
        Task DisableFromKeycloakAsync(string userId, CancellationToken ct);
        Task EnableFromKeycloakAsync(string userId, CancellationToken ct);
    }
}
