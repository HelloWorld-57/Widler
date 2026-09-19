using Npgsql;
using UsersService.Application.Interfaces;

namespace UsersService.Infrastructure.Db
{
    public sealed class PostgresReconciliationLock : IReconciliationLock, IAsyncDisposable
    {
        private const long LockKey = 7_314_159;

        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<PostgresReconciliationLock> _logger;

        private NpgsqlConnection? _connection;
        private bool _lockAcquired;

        public PostgresReconciliationLock(NpgsqlDataSource dataSource, ILogger<PostgresReconciliationLock> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task<bool> TryAcquireAsync(CancellationToken ct)
        {
            if (_lockAcquired)
            {
                return true;
            }

            var connection = await _dataSource.OpenConnectionAsync(ct);

            await using var command = new NpgsqlCommand("SELECT pg_try_advisory_lock(@lockKey);", connection);

            command.Parameters.AddWithValue("lockKey", LockKey);

            var result = await command.ExecuteScalarAsync(ct);

            var acquired = result is true;

            if (!acquired)
            {
                await connection.DisposeAsync();

                _logger.LogDebug("Reconciliation lock is already held by another UsersService instance");

                return false;
            }

            _connection = connection;
            _lockAcquired = true;

            _logger.LogInformation("Reconciliation lock acquired. LockKey={LockKey}", LockKey);

            return true;
        }

        public async Task ReleaseAsync(CancellationToken ct)
        {
            if (!_lockAcquired || _connection is null)
            {
                return;
            }

            try
            {
                await using var command = new NpgsqlCommand("SELECT pg_advisory_unlock(@lockKey);", _connection);

                command.Parameters.AddWithValue("lockKey", LockKey);

                await command.ExecuteScalarAsync(ct);

                _logger.LogInformation("Reconciliation lock released. LockKey={LockKey}", LockKey);
            }
            finally
            {
                await _connection.DisposeAsync();
                _connection = null;
                _lockAcquired = false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }

            _lockAcquired = false;
        }
    }
}
