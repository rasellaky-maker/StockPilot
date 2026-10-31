using BO.Models;
using MySqlConnector;
using System.Data;

namespace DAL
{
    public class CoinRepository
    {
        private string _connectionString = string.Empty;
        public CoinRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Retrieves a list of all coins from the database.
        /// </summary>
        /// <returns>A list of all Coin objects from the coin table.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the database service is unavailable or an error occurs while reading data.</exception>
        public async Task<List<Coin>> SelectAsync()
        {
            var result = new List<Coin>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                SELECT id, name, value, numberofbuyers, image, userid
                FROM coin
                ORDER BY numberofbuyers DESC;
                ";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(CoinReaderMap(reader));
                }

                return result;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while reading data from the database.", ex);
            }

        }

        /// <summary>
        /// Retrieves a coin by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the coin.</param>
        /// <returns>A Coin object if found; otherwise null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when id is less than or equal to 0.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the database service is unavailable or an error occurs while reading data.</exception>
        public async Task<Coin?> SelectByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                SELECT id, name, value, numberofbuyers, image, userid
                FROM coin
                WHERE id = @id;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return CoinReaderMap(reader);
                }

                return null;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while reading data from the database.", ex);
            }
        }
        public async Task<Coin?> SelectByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                // Fix: "=" statt LIKE. Die Methode prüft den genauen Namen (z. B. beim Anlegen einer Coin);
                // bei LIKE würden % und _ im Namen als Platzhalter wirken.
                const string query = @"
                    SELECT id, name, value, numberofbuyers, image, userid
                    FROM coin
                    WHERE name = @name;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@name", MySqlDbType.VarChar, 255).Value = name;
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return CoinReaderMap(reader);
                }
                return null;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while reading data from the database.", ex);
            }
        }
        public async Task<List<Coin>> SearchAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var result = new List<Coin>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT id, name, value, numberofbuyers, image, userid
                    FROM coin
                    WHERE name LIKE @name
                    ORDER BY name;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = "%" + name + "%";
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(CoinReaderMap(reader));
                }

                return result;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while reading data from the database.", ex);
            }
        }

        /// <summary>
        /// Inserts a new coin into the database.
        /// </summary>
        /// <param name="coin">The Coin object to insert.</param>
        /// <returns>The inserted Coin object with the generated Id.</returns>
        /// <exception cref="ArgumentNullException">Thrown when coin is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the database service is unavailable or an error occurs while inserting data.</exception>
        public async Task<Coin> InsertAsync(Coin coin)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                INSERT INTO coin(name, value, numberofbuyers, image, userid) 
                VALUES(@name, @value, @numberofbuyers, @image, @userid);
                SELECT LAST_INSERT_ID();
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@name", MySqlDbType.VarChar, 255).Value = coin.Name;
                cmd.Parameters.Add("@value", MySqlDbType.Decimal).Value = coin.Value;
                cmd.Parameters.Add("@numberofbuyers", MySqlDbType.Int32).Value = coin.NumberOfBuyers;
                // Fix: Spalte image ist LONGTEXT (Base64-Bilder sind lang), deshalb LongText ohne Größenlimit.
                cmd.Parameters.Add("@image", MySqlDbType.LongText).Value = coin.Image ?? (object)DBNull.Value;
                // UserId <= 0 bedeutet "kein Besitzer": als NULL speichern (0 würde den Fremdschlüssel verletzen).
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value =
                    coin.UserId > 0 ? coin.UserId : (object)DBNull.Value;

                var id = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                coin.Id = (int)id;

                return coin;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while inserting data into the database.", ex);
            }
        }
        public async Task<int> UpdateValueAsync(int id, decimal value)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    UPDATE coin SET value = @value
                    WHERE id = @id;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@value", MySqlDbType.Decimal).Value = value;
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                return await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while updating data in the database.", ex);
            }
        }
        public async Task<int> RandomizeValuesAsync(decimal maxPercent)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    UPDATE coin
                    SET value = GREATEST(0.01, ROUND(value * (1 + (RAND() * 2 - 1) * @p / 100), 2));
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@p", MySqlDbType.Decimal).Value = maxPercent;

                return await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while updating data in the database.", ex);
            }
        }
        /// <summary>
        /// Updates an existing coin in the database.
        /// </summary>
        /// <param name="id">The unique identifier of the coin to update.</param>
        /// <param name="coin">The updated Coin object.</param>
        /// <returns>The number of affected rows.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when id is less than or equal to 0.</exception>
        /// <exception cref="ArgumentNullException">Thrown when coin is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the database service is unavailable or an error occurs while updating data.</exception>
        public async Task<int> UpdateAsync(int id, Coin coin)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                UPDATE coin SET 
                name = @name,
                value = @value,
                numberofbuyers = @numberofbuyers,
                image = @image,
                userid = @userid
                WHERE id = @id;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@name", MySqlDbType.VarChar, 255).Value = coin.Name;
                cmd.Parameters.Add("@value", MySqlDbType.Decimal).Value = coin.Value;
                cmd.Parameters.Add("@numberofbuyers", MySqlDbType.Int32).Value = coin.NumberOfBuyers;
                cmd.Parameters.Add("@image", MySqlDbType.LongText).Value = coin.Image ?? (object)DBNull.Value;
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value =
                    coin.UserId > 0 ? coin.UserId : (object)DBNull.Value;
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;

                return await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while updating data in the database.", ex);
            }
        }

        /// <summary>
        /// Maps a database reader row to a Coin object.
        /// </summary>
        /// <param name="reader">The MySqlDataReader to map from.</param>
        /// <returns>A new Coin object with data from the reader.</returns>
        private Coin CoinReaderMap(MySqlDataReader reader)
        {
            return new Coin(
                reader.GetInt32("id"),
                reader.GetString("name"),
                reader.GetDecimal("value"),
                reader.GetInt32("numberofbuyers"),
                reader.IsDBNull("image") ? null : reader.GetString("image"),
                // Fix: userid kann in der DB NULL sein (z. B. bei bereits vorhandenen Coins): NULL -> 0
                reader.IsDBNull("userid") ? 0 : reader.GetInt32("userid")
            );
        }
    }
}