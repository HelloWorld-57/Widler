using Microsoft.Extensions.Options;
using System.Diagnostics;
using UsersService.Application.Interfaces;
using UsersService.Infrastructure.Keycloak;
using UsersService.Infrastructure.Observability;

namespace UsersService.Infrastructure.BackgroundServices
{
    public sealed class UserReconciliationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<KeycloakReconciliationOptions> _options;
        private readonly IReconciliationLock _reconciliationLock;
        private readonly ILogger<UserReconciliationWorker> _logger;

        public UserReconciliationWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<KeycloakReconciliationOptions> options,
            IReconciliationLock reconciliationLock,
            ILogger<UserReconciliationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _reconciliationLock = reconciliationLock;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(_options.Value.IntervalMinutes);

            _logger.LogInformation("User reconciliation worker started. Interval={Interval}", interval);

            await RunReconciliationAsync(stoppingToken);

            using var timer = new PeriodicTimer(interval);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await RunReconciliationAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("User reconciliation worker cancellation requested");
            }

            _logger.LogInformation("User reconciliation worker stopped");
        }

        private async Task RunReconciliationAsync(CancellationToken ct)
        {
            var lockAcquired = false;
            
            try
            {
                lockAcquired = await _reconciliationLock.TryAcquireAsync(ct);

                if (!lockAcquired)
                {
                    Metrics.ReconciliationSkipped.Add(1);
                    _logger.LogInformation("Skipping reconciliation because another UsersService instance is running it");

                    return;
                }

                var stopwatch = Stopwatch.StartNew();

                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();

                    var reconciliation = scope.ServiceProvider.GetRequiredService<IUserReconciliationService>();

                    var result = await reconciliation.ReconcileAsync(ct);

                    stopwatch.Stop();

                    Metrics.ReconciliationRuns.Add(1);
                    Metrics.ReconciliationUsersCreated.Add(result.Created);
                    Metrics.ReconciliationUsersUpdated.Add(result.Updated);
                    Metrics.ReconciliationUsersRestored.Add(result.Restored);
                    Metrics.ReconciliationUsersDeleted.Add(result.Deleted);
                    Metrics.ReconciliationDuration.Record(stopwatch.Elapsed.TotalMilliseconds);

                    _logger.LogInformation(
                        "Keycloak users reconciliation finished. " +
                        "DurationMs={DurationMs}, " +
                        "KeycloakUsers={KeycloakUsers}, " +
                        "LocalUsers={LocalUsers}, " +
                        "Created={Created}, " +
                        "Restored={Restored}, " +
                        "Updated={Updated}, " +
                        "Deleted={Deleted}",
                        stopwatch.Elapsed.TotalMilliseconds,
                        result.KeycloakUsers,
                        result.LocalUsers,
                        result.Created,
                        result.Restored,
                        result.Updated,
                        result.Deleted);
                }
                catch {
                    stopwatch.Stop();

                    Metrics.ReconciliationDuration.Record(stopwatch.Elapsed.TotalMilliseconds);

                    throw;
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Normal application shutdown.
            }
            catch (Exception ex)
            {
                Metrics.ReconciliationErrors.Add(1);
                _logger.LogError(ex, "User reconciliation failed");
            }
            finally
            {
                if (lockAcquired)
                {
                    try
                    {
                        await _reconciliationLock.ReleaseAsync(CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to release reconciliation lock");
                    }
                }
            }
        }
    }
}
