
using BO.Models;
using DAL;

namespace BLL.CoreServisecs
{
    public class CoinPriceHistoryService
    {
        private readonly CoinPriceHistoryRepository _repo;

        public CoinPriceHistoryService(string connectionString)
        {
            _repo = new CoinPriceHistoryRepository(connectionString);
        }

        private static readonly Dictionary<string, (TimeSpan Span, int BucketSeconds)> Ranges =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["1h"] = (TimeSpan.FromHours(1), 10),
                ["24h"] = (TimeSpan.FromHours(24), 120),
                ["7d"] = (TimeSpan.FromDays(7), 900),
                ["30d"] = (TimeSpan.FromDays(30), 3600)
            };

        public async Task<List<bool>> CreateAsync(
            IEnumerable<CoinPriceHistory> coinPriceHistory)
        {
            ArgumentNullException.ThrowIfNull(coinPriceHistory);

            return await _repo.InsertManyAsync(coinPriceHistory);
        }

        public async Task<List<bool>> RecordSnapshotAsync(
            IEnumerable<Coin> coins)
        {
            ArgumentNullException.ThrowIfNull(coins);

            var now = DateTime.UtcNow;

            var snapshot = coins
                .Where(c => c != null && c.Id > 0 && c.Value > 0)
                .Select(c => new CoinPriceHistory
                {
                    Coin = c,
                    Price = c.Value,
                    RecordedAt = now
                })
                .ToList();

            if (snapshot.Count == 0)
                return new List<bool>();

            return await _repo.InsertManyAsync(snapshot);
        }

        public async Task<List<PricePoint>> GetHistoryAsync(
            int coinId,
            string range = "1h")
        {
            if (coinId <= 0)
                throw new ArgumentOutOfRangeException(nameof(coinId));

            if (string.IsNullOrWhiteSpace(range) ||
                !Ranges.TryGetValue(range, out var config))
            {
                throw new ArgumentException(
                    $"Unknown range '{range}'. Allowed: {string.Join(", ", Ranges.Keys)}"
                );
            }

            var fromUtc = DateTime.UtcNow.Subtract(config.Span);

            return await _repo.GetHistoryAggregatedAsync(
                coinId,
                fromUtc,
                config.BucketSeconds
            );
        }
    }
}
