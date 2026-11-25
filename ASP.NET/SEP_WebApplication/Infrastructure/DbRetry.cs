using MySqlConnector;

namespace SEP_WebApplication.Infrastructure
{
    /// <summary>
    /// Retries READ-ONLY database calls after a short pause. A pooled connection that was
    /// closed by the server (idle timeout of the hoster) is replaced by the pool on the next try.
    /// Never use this for writes (buy / sell / create) - they are not idempotent.
    /// </summary>
    public static class DbRetry
    {
        public static async Task<T> RunAsync<T>(Func<Task<T>> action, int maxAttempts = 3)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (attempt < maxAttempts && IsDbFailure(ex))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt));
                }
            }
        }

        private static bool IsDbFailure(Exception ex) =>
            ex is MySqlException or InvalidOperationException or TimeoutException or IOException;
    }
}
