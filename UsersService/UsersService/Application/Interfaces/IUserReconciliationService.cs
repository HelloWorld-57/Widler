using UsersService.DTOs;

namespace UsersService.Application.Interfaces
{
    public interface IUserReconciliationService
    {
        Task<ReconciliationResult> ReconcileAsync(CancellationToken ct);
    }
}
