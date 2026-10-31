using BO.Models;
using MySqlConnector;
using System.Data;

namespace DAL
{
    public class UsersCoinsRepository
    {
        private string _connectionString = string.Empty;
        public UsersCoinsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<UsersCoins>> SelectAsync()
        {
            var result = new List<UsersCoins>();
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT uc.id AS usercoin_id, uc.quantity, uc.acquisitiondate,
                           c.id AS coin_id, c.name AS coin_name, c.value, c.numberofbuyers, c.image, c.userid AS coin_userid,
                           u.id AS user_id, u.username, u.passhash, u.passsalt, u.email, u.availablecoins
                    FROM userscoins uc
                    INNER JOIN coin c ON c.id = uc.coinid
                    INNER JOIN user u ON u.id = uc.userid;
                ";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(UsersCoinsReaderMap(reader));
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
        public async Task<List<UsersCoins>> SelectByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            var result = new List<UsersCoins>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT MIN(uc.id) AS usercoin_id,
                           SUM(uc.quantity) AS quantity,
                           MAX(uc.acquisitiondate) AS acquisitiondate,
                           c.id AS coin_id, c.name AS coin_name, c.value, c.numberofbuyers, c.image, c.userid AS coin_userid,
                           u.id AS user_id, u.username, u.passhash, u.passsalt, u.email, u.availablecoins
                    FROM userscoins uc
                    INNER JOIN coin c ON c.id = uc.coinid
                    INNER JOIN user u ON u.id = uc.userid
                    WHERE uc.userid = @id
                    GROUP BY c.id, c.name, c.value, c.numberofbuyers, c.image, c.userid,
                             u.id, u.username, u.passhash, u.passsalt, u.email, u.availablecoins
                    ORDER BY c.id;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = userId;
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(UsersCoinsReaderMap(reader));
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
        public async Task<UsersCoins> InsertAsync(UsersCoins usersCoin)
        {
            if (usersCoin == null)
                throw new ArgumentNullException(nameof(usersCoin));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    INSERT INTO userscoins(userid, coinid, quantity, AcquisitionDate) 
                    VALUES(@userid, @coinid, @quantity, @acquisitiondate);
                    SELECT LAST_INSERT_ID();
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = usersCoin.User.Id;
                cmd.Parameters.Add("@coinid", MySqlDbType.Int32).Value = usersCoin.Coin.Id;
                cmd.Parameters.Add("@quantity", MySqlDbType.Decimal).Value = usersCoin.Quantity;
                cmd.Parameters.Add("@acquisitiondate", MySqlDbType.DateTime).Value = usersCoin.AcquisitionDate;

                var id = await cmd.ExecuteScalarAsync();
                usersCoin.Id = Convert.ToInt32(id);

                return usersCoin;
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
        public async Task<int> UpdateAsync(int id, UsersCoins usersCoins)
        {
            if (usersCoins == null)
                throw new ArgumentNullException(nameof(usersCoins));
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                using var conn = new MySqlConnection(_connectionString);

                await conn.OpenAsync();

                const string query = @"
                UPDATE userscoins SET userid = @userid,
                    coinid = @coinid,
                    quantity = @quantity,
                    acquisitiondate = @acquisitiondate
                WHERE id = @id;
            ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = usersCoins.User.Id;
                cmd.Parameters.Add("@coinid", MySqlDbType.Int32).Value = usersCoins.Coin.Id;
                cmd.Parameters.Add("@quantity", MySqlDbType.Decimal).Value = usersCoins.Quantity;
                cmd.Parameters.Add("@acquisitiondate", MySqlDbType.DateTime).Value = usersCoins.AcquisitionDate;
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
        public async Task<int> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string quety = @"
                    DELETE FROM userscoins
                    WHERE id = @id;
                ";

                using var cmd = new MySqlCommand(quety, conn);
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while deleting data in the database.", ex);
            }
        }

        public async Task<bool> BuyAsync(int userId, int coinId, decimal quantity, decimal cost,
                                         int ownerId = 0, decimal ownerShare = 0)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (coinId <= 0) throw new ArgumentOutOfRangeException(nameof(coinId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (cost < 0) throw new ArgumentOutOfRangeException(nameof(cost));
            if (ownerShare < 0 || ownerShare > cost) throw new ArgumentOutOfRangeException(nameof(ownerShare));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                using var tx = await conn.BeginTransactionAsync();

                using (var cmd = new MySqlCommand(
                    "UPDATE wallet SET balance = balance - @cost WHERE userid = @userid AND balance >= @cost;",
                    conn, tx))
                {
                    cmd.Parameters.Add("@cost", MySqlDbType.Decimal).Value = cost;
                    cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = userId;

                    if (await cmd.ExecuteNonQueryAsync() == 0)
                    {
                        await tx.RollbackAsync();
                        return false;
                    }
                }

                if (ownerId > 0 && ownerShare > 0)
                {
                    using var cmd = new MySqlCommand(
                        "UPDATE wallet SET balance = balance + @share WHERE userid = @ownerid;",
                        conn, tx);
                    cmd.Parameters.Add("@share", MySqlDbType.Decimal).Value = ownerShare;
                    cmd.Parameters.Add("@ownerid", MySqlDbType.Int32).Value = ownerId;
                    await cmd.ExecuteNonQueryAsync();
                }

                bool isNewBuyer;
                using (var cmd = new MySqlCommand(
                    "SELECT EXISTS(SELECT 1 FROM userscoins WHERE userid = @userid AND coinid = @coinid);",
                    conn, tx))
                {
                    cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = userId;
                    cmd.Parameters.Add("@coinid", MySqlDbType.Int32).Value = coinId;
                    isNewBuyer = Convert.ToInt32(await cmd.ExecuteScalarAsync()) == 0;
                }

                using (var cmd = new MySqlCommand(@"
            INSERT INTO userscoins(userid, coinid, quantity, acquisitiondate)
            VALUES(@userid, @coinid, @qty, @date);", conn, tx))
                {
                    cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = userId;
                    cmd.Parameters.Add("@coinid", MySqlDbType.Int32).Value = coinId;
                    cmd.Parameters.Add("@qty", MySqlDbType.Decimal).Value = quantity;
                    cmd.Parameters.Add("@date", MySqlDbType.DateTime).Value = DateTime.Now;
                    await cmd.ExecuteNonQueryAsync();
                }

                if (isNewBuyer)
                {
                    using var cmd = new MySqlCommand(
                        "UPDATE coin SET numberofbuyers = numberofbuyers + 1 WHERE id = @coinid;", conn, tx);
                    cmd.Parameters.Add("@coinid", MySqlDbType.Int32).Value = coinId;
                    await cmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return true;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
        }

        /// <summary>
        /// Verkauft Coins zurück an die Börse zum aktuellen Preis.
        /// Alles in einer Transaktion: Bestand verringern, Erlös aufs Wallet, ggf. Käuferzahl senken.
        /// </summary>
        /// <returns>Der Erlös, oder null wenn der Verkauf nicht möglich war
        /// (zu wenig Coins, Coin oder Wallet nicht gefunden).</returns>
        public async Task<decimal?> SellAsync(int userId, int coinId, decimal quantity)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            if (coinId <= 0) throw new ArgumentOutOfRangeException(nameof(coinId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();
                using var tx = await conn.BeginTransactionAsync();

                async Task ExecAsync(string sql, Action<MySqlParameterCollection> bind)
                {
                    using var c = new MySqlCommand(sql, conn, tx);
                    bind(c.Parameters);
                    await c.ExecuteNonQueryAsync();
                }

                var holdings = new List<(int Id, decimal Quantity)>();
                using (var cmd = new MySqlCommand(@"
                    SELECT id, quantity
                    FROM userscoins
                    WHERE userid = @userid AND coinid = @coinid
                    ORDER BY acquisitiondate, id
                    FOR UPDATE;", conn, tx))
                {
                    cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = userId;
                    cmd.Parameters.Add("@coinid", MySqlDbType.Int32).Value = coinId;

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        holdings.Add((reader.GetInt32("id"), reader.GetDecimal("quantity")));
                    }
                }

                decimal total = 0;
                foreach (var h in holdings)
                    total += h.Quantity;

                if (total < quantity)
                {
                    await tx.RollbackAsync();
                    return null;
                }

                decimal price;
                using (var cmd = new MySqlCommand("SELECT value FROM coin WHERE id = @coinid;", conn, tx))
                {
                    cmd.Parameters.Add("@coinid", MySqlDbType.Int32).Value = coinId;
                    var value = await cmd.ExecuteScalarAsync();

                    if (value is null || value is DBNull)
                    {
                        await tx.RollbackAsync();
                        return null;
                    }

                    price = Convert.ToDecimal(value);
                }

                decimal revenue = Math.Round(price * quantity, 8);

                decimal remaining = quantity;
                foreach (var h in holdings)
                {
                    if (remaining <= 0)
                        break;

                    if (h.Quantity <= remaining)
                    {
                        await ExecAsync("DELETE FROM userscoins WHERE id = @id;",
                            p => p.Add("@id", MySqlDbType.Int32).Value = h.Id);
                        remaining -= h.Quantity;
                    }
                    else
                    {
                        await ExecAsync("UPDATE userscoins SET quantity = quantity - @qty WHERE id = @id;", p =>
                        {
                            p.Add("@qty", MySqlDbType.Decimal).Value = remaining;
                            p.Add("@id", MySqlDbType.Int32).Value = h.Id;
                        });
                        remaining = 0;
                    }
                }

                using (var cmd = new MySqlCommand(
                    "UPDATE wallet SET balance = balance + @revenue WHERE userid = @userid;", conn, tx))
                {
                    cmd.Parameters.Add("@revenue", MySqlDbType.Decimal).Value = revenue;
                    cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = userId;

                    if (await cmd.ExecuteNonQueryAsync() == 0)
                    {
                        await tx.RollbackAsync();
                        return null;
                    }
                }

                if (total == quantity)
                {
                    await ExecAsync(
                        "UPDATE coin SET numberofbuyers = GREATEST(numberofbuyers - 1, 0) WHERE id = @coinid;",
                        p => p.Add("@coinid", MySqlDbType.Int32).Value = coinId);
                }

                await tx.CommitAsync();
                return revenue;
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException("The database service is unavailable.", ex);
            }
        }

        private UsersCoins UsersCoinsReaderMap(MySqlDataReader reader)
        {
            return new UsersCoins(
                reader.GetInt32("usercoin_id"),
                new User(
                    reader.GetInt32("user_id"),
                    reader.GetString("username"),
                    reader.GetString("passhash"),
                    reader.GetString("passsalt"),
                    reader.GetString("email"),
                    reader.GetByte("availablecoins")
                ),
                new Coin(
                    reader.GetInt32("coin_id"),
                    reader.GetString("coin_name"),
                    reader.GetDecimal("value"),
                    reader.GetInt32("numberofbuyers"),
                    reader.IsDBNull("image") ? null : reader.GetString("image"),
                    reader.IsDBNull("coin_userid") ? 0 : reader.GetInt32("coin_userid")
                ),
                reader.GetDecimal("quantity"),
                reader.GetDateTime("acquisitiondate")
            );
        }
    }
}