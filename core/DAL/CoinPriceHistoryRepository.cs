using BO.Models;
using MySqlConnector;
using System.Security.Cryptography.X509Certificates;

namespace DAL
{
    public class CoinPriceHistoryRepository
    {
        private string _connectionString = string.Empty;
        public CoinPriceHistoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<List<PricePoint>> GetHistoryAggregatedAsync(int coinId, DateTime fromUtc, int bucketSeconds)
        {
            var result = new List<PricePoint>();

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
            SELECT
                DATE_ADD('1970-01-01',
                    INTERVAL FLOOR(TIMESTAMPDIFF(SECOND, '1970-01-01', RecordedAt) / @bucket) * @bucket SECOND) AS bucket_time,
                AVG(Price) AS avg_price
            FROM CoinPriceHistory
            WHERE CoinId = @coinId
              AND RecordedAt >= @from
            GROUP BY bucket_time
            ORDER BY bucket_time;";

                await using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@coinId", MySqlDbType.Int32).Value = coinId;
                cmd.Parameters.Add("@from", MySqlDbType.DateTime).Value = fromUtc;
                cmd.Parameters.Add("@bucket", MySqlDbType.Int32).Value = bucketSeconds;

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new PricePoint
                    {
                        Time = DateTime.SpecifyKind(reader.GetDateTime("bucket_time"), DateTimeKind.Utc),
                        Price = reader.GetDecimal("avg_price")
                    });
                }

                return result;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(
                    $"Datenbankfehler beim Laden der Preishistorie (CoinId {coinId}).", ex);
            }
        }
        public async Task<List<bool>> InsertManyAsync(IEnumerable<CoinPriceHistory> items)
        {
            ArgumentNullException.ThrowIfNull(items);
            var results = new List<bool>();

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    INSERT INTO coinpricehistory (coinid, price, recordedat)
                    VALUES (@coinid, @price, @recordedat);
                ";

                await using var cmd = new MySqlCommand(query, conn);
                var pCoin = cmd.Parameters.Add("@coinid", MySqlDbType.Int32);
                var pPrice = cmd.Parameters.Add("@price", MySqlDbType.Decimal);
                var pTime = cmd.Parameters.Add("@recordedat", MySqlDbType.DateTime);

                foreach (var cph in items)
                {
                    if (cph?.Coin == null)
                    {
                        results.Add(false);
                        continue;
                    }

                    pCoin.Value = cph.Coin.Id;
                    pPrice.Value = cph.Price;
                    pTime.Value = cph.RecordedAt == default ? DateTime.UtcNow : cph.RecordedAt;

                    try
                    {
                        results.Add(await cmd.ExecuteNonQueryAsync() == 1);
                    }
                    catch (MySqlException)
                    {
                        results.Add(false);
                    }
                }

                return results;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("Datenbankfehler beim Speichern der Preishistorie.", ex);
            }
        }
    }
}
