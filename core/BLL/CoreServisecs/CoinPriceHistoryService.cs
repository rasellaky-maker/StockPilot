using BO.Models;
using DAL;

namespace BLL.CoreServisecs
{
    public class CoinPriceHistoryService
    {
        private CoinPriceHistoryRepository _repo = null;
        public CoinPriceHistoryService(string connectionString)
        {
            _repo = new CoinPriceHistoryRepository(connectionString);
        }
        public async Task<List<bool>> CreateAsync(IEnumerable<CoinPriceHistory> coinPriceHistory)
        {
            if (coinPriceHistory == null)
                throw new ArgumentNullException(nameof(coinPriceHistory));

            return await _repo.InsertManyAsync(coinPriceHistory);
        }

        public async Task<List<bool>> RecordSnapshotAsync(IEnumerable<Coin> coins)
        {
            if (coins == null)
                throw new ArgumentNullException(nameof(coins));

            var now = DateTime.UtcNow;

            var snapshot = coins.Select(c => new CoinPriceHistory
            {
                Coin = c,
                Price = c.Value,
                RecordedAt = now
            }).ToList();

            return await _repo.InsertManyAsync(snapshot);
        }

        private static readonly Dictionary<string, (TimeSpan Span, int BucketSeconds)> Ranges =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["1h"] = (TimeSpan.FromHours(1), 10),
                ["24h"] = (TimeSpan.FromHours(24), 120),
                ["7d"] = (TimeSpan.FromDays(7), 900),
                ["30d"] = (TimeSpan.FromDays(30), 3600)
            };

        public async Task<List<PricePoint>> GetHistoryAsync(int coinId, string range = "1h")
        {
            if (coinId <= 0)
                throw new ArgumentOutOfRangeException(nameof(coinId));

            if (!Ranges.TryGetValue(range ?? "", out var cfg))
                throw new ArgumentException($"Unknown range '{range}'. Allowed: {string.Join(", ", Ranges.Keys)}", nameof(range));

            var fromUtc = DateTime.UtcNow - cfg.Span;
            return await _repo.GetHistoryAggregatedAsync(coinId, fromUtc, cfg.BucketSeconds);
        }
    }
}
