
using BO.Models;
using MySqlConnector;

namespace DAL
{
    public class CoinPriceHistoryRepository
    {
        private readonly string _connectionString;

        public CoinPriceHistoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<List<PricePoint>> GetHistoryAggregatedAsync(
            int coinId,
            DateTime fromUtc,
            int bucketSeconds)
        {
            var result = new List<PricePoint>();

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT
                        DATE_ADD(
                            '1970-01-01',
                            INTERVAL (
                                FLOOR(
                                    TIMESTAMPDIFF(
                                        SECOND,
                                        '1970-01-01',
                                        recordedat
                                    ) / @bucket
                                ) * @bucket
                            ) SECOND
                        ) AS bucket_time,
                        AVG(price) AS avg_price
                    FROM coinpricehistory
                    WHERE coinid = @coinid
                      AND recordedat >= @from
                    GROUP BY bucket_time
                    ORDER BY bucket_time ASC;";

                await using var cmd = new MySqlCommand(query, conn);

                cmd.Parameters.Add("@coinid", MySqlDbType.Int32).Value = coinId;
                cmd.Parameters.Add("@from", MySqlDbType.DateTime).Value = fromUtc;
                cmd.Parameters.Add("@bucket", MySqlDbType.Int32).Value = bucketSeconds;

                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (reader.IsDBNull(0) || reader.IsDBNull(1))
                        continue;

                    result.Add(new PricePoint
                    {
                        Time = DateTime.SpecifyKind(
                            reader.GetDateTime(0),
                            DateTimeKind.Utc
                        ),

                        Price = reader.GetDecimal(1)
                    });
                }

                return result;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(
                    $"Database error loading history for coin {coinId}.",
                    ex
                );
            }
        }
        public async Task<List<bool>> InsertManyAsync(
            IEnumerable<CoinPriceHistory> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            var results = new List<bool>();

            try
            {
                await using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    INSERT INTO coinpricehistory
                        (coinid, price, recordedat)
                    VALUES
                        (@coinid, @price, @recordedat);";

                await using var cmd = new MySqlCommand(query, conn);

                var pCoin = cmd.Parameters.Add("@coinid", MySqlDbType.Int32);
                var pPrice = cmd.Parameters.Add("@price", MySqlDbType.Decimal);
                var pTime = cmd.Parameters.Add("@recordedat", MySqlDbType.DateTime);

                foreach (var item in items)
                {
                    if (item?.Coin == null ||
                        item.Coin.Id <= 0 ||
                        item.Price <= 0)
                    {
                        results.Add(false);
                        continue;
                    }

                    pCoin.Value = item.Coin.Id;
                    pPrice.Value = item.Price;

                    pTime.Value = item.RecordedAt == default
                        ? DateTime.UtcNow
                        : item.RecordedAt.ToUniversalTime();

                    try
                    {
                        results.Add(
                            await cmd.ExecuteNonQueryAsync() == 1
                        );
                    }
                    catch (MySqlException ex)
                    {
                        throw new InvalidOperationException("Invalid operation", ex);
                    }
                }

                return results;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException(
                    "Database error saving price history.",
                    ex
                );
            }
        }
    }
}
