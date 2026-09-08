using UsersService.Application.Commands;
using UsersService.DTOs.Response;

namespace UsersService.Application.Interfaces
{
    public interface IUserService
    {
        Task<IReadOnlyCollection<UserResponse>> GetAllAsync();
        Task<UserResponse> GetByIdAsync(string id);
        //Task<string> CreateAsync(CreateUserCommand command);
        Task UpdateAsync(UpdateUserCommand command);
        Task ReplaceAsync(ReplaceUserCommand command);
        Task DeleteAsync(string id);

        Task CreateFromKeycloakAsync(CreateUserFromKeycloakCommand cmd, CancellationToken ct);
        Task UpdateEmailFromKeycloakAsync(UpdateUserEmailFromKeycloakCommand cmd, CancellationToken ct);
        Task DeleteFromKeycloakAsync(string userId, CancellationToken ct);
    }
}
