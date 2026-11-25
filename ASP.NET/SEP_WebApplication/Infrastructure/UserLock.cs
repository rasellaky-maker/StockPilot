using System.Collections.Concurrent;

namespace SEP_WebApplication.Infrastructure
{
    /// <summary>
    /// Serialises read-modify-write sequences per user (e.g. creating a coin decreases AvailableCoins),
    /// so two parallel requests cannot both pass the "has coins left" check.
    /// </summary>
    public static class UserLock
    {
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> Locks = new();

        public static async Task<IDisposable> AcquireAsync(int userId, CancellationToken ct = default)
        {
            var semaphore = Locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(ct);
            return new Releaser(semaphore);
        }

        private sealed class Releaser : IDisposable
        {
            private SemaphoreSlim? _semaphore;

            public Releaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

            public void Dispose() => Interlocked.Exchange(ref _semaphore, null)?.Release();
        }
    }
}
