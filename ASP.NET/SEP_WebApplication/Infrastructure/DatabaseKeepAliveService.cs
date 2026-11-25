using MySqlConnector;

namespace SEP_WebApplication.Infrastructure
{
    /// <summary>
    /// Warms up the connection pool at start-up and pings the database periodically, so one pooled
    /// connection is always alive and the hoster never drops it as "idle".
    /// Failures are only logged - this service must never stop the application.
    /// </summary>
    public sealed class DatabaseKeepAliveService : BackgroundService
    {
        private readonly DbSettings _settings;
        private readonly ILogger<DatabaseKeepAliveService> _logger;

        public DatabaseKeepAliveService(DbSettings settings, ILogger<DatabaseKeepAliveService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PingAsync(_settings.ConnectionString, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Database keep-alive ping failed.");
                }

                try
                {
                    await Task.Delay(_settings.KeepAliveInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public static async Task PingAsync(string connectionString, CancellationToken ct)
        {
            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(ct);
            await using var command = new MySqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(ct);
        }
    }
}
