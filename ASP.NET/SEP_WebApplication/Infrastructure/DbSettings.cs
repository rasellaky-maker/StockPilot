namespace SEP_WebApplication.Infrastructure
{
    /// <summary>
    /// Final (pool-tuned) connection string and keep-alive interval.
    /// Registered once as a singleton in Program.cs.
    /// </summary>
    public sealed record DbSettings(string ConnectionString, TimeSpan KeepAliveInterval);
}
