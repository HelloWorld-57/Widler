using UsersService.Domain.Entities;

namespace UsersService.Application.Interfaces
{
    public interface IUserCache
    {
        Task<User?> GetAsync(string userId, CancellationToken cancellationToken = default);

        Task SetAsync(User user, CancellationToken cancellationToken = default);

        Task RemoveAsync(string userId, CancellationToken cancellationToken = default);
    }
}
