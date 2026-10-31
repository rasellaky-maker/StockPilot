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

        public CoinPriceUpdater(ILogger<CoinPriceUpdater> logger, string connectionString)
        {
            _logger = logger;

            var cs = ConnectionStringProvider.GetConnectionString(connectionString);
            _coinService = new CoinService(cs);
            _historyService = new CoinPriceHistoryService(cs);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _coinService.RandomizeValuesAsync(3m);

                    var coins = await _coinService.GetAsync();
                    var results = await _historyService.RecordSnapshotAsync(coins);

                    var failed = results.Count(r => !r);
                    if (failed > 0)
                        _logger.LogWarning("Preishistorie: {Failed} von {Total} Einträgen nicht gespeichert",
                            failed, results.Count);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Aktualisieren der Coin-Werte");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(_random.Next(4000, 5001)), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}