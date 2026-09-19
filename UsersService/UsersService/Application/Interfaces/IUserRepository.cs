using UsersService.Domain.Entities;

namespace UsersService.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<IReadOnlyCollection<User>> GetAllIncludingDeletedAsync(CancellationToken ct = default);
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByIdIncludingDeletedAsync(string id, CancellationToken ct = default);
        Task AddAsync(User user);
        //Task UpdateAsync(User user);
        Task SaveChangesAsync();
        Task SaveChangesAsync(CancellationToken ct);
    }
}