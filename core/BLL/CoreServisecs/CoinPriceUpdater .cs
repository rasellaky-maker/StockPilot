using BLL.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BLL.CoreServisecs
{
    public class CoinPriceUpdater : BackgroundService
    {
        private readonly CoinService _coinService;
        private readonly CoinPriceHistoryService _historyService;
        private readonly ILogger<CoinPriceUpdater> _logger;
        private readonly Random _random = new();

        public CoinPriceUpdater(
            ILogger<CoinPriceUpdater> logger,
            string connectionName)
        {
            _logger = logger;
            var cs = ConnectionStringProvider.GetConnectionString(connectionName);

            _coinService = new CoinService(cs);
            _historyService = new CoinPriceHistoryService(cs);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CoinPriceUpdater started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var updated = await _coinService.RandomizeValuesAsync(3m);

                    _logger.LogInformation(
                        "Updated coins: {Count}", updated);

                    var coins = await _coinService.GetAsync();

                    var results = await _historyService.RecordSnapshotAsync(coins);

                    _logger.LogInformation(
                        "Price history saved: {Count}", results.Count(r => r));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Coin updater failed.");
                }

                try
                {
                    await Task.Delay(5000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}