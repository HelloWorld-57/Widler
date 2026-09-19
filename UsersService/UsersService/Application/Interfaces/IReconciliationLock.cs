namespace UsersService.Application.Interfaces
{
    public interface IReconciliationLock
    {
        Task<bool> TryAcquireAsync(CancellationToken ct);

        Task ReleaseAsync(CancellationToken ct);
    }
}
