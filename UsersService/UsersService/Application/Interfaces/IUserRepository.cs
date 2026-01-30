using UsersService.Domain.Entities;

namespace UsersService.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task AddAsync(User user);
        //Task UpdateAsync(User user);
        Task SaveChangesAsync();
        Task SaveChangesAsync(CancellationToken ct);
    }
}