using BO.Models;
using MySqlConnector;

namespace DAL
{
    public class WalletRepository
    {
        private string _connectionString = string.Empty;

        public WalletRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Wallet>> SelectAsync()
        {
            var result = new List<Wallet>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT w.id AS wallet_id, w.balance, u.id AS user_id, u.username, u.passhash, u.passsalt, u.email, u.availablecoins
                    FROM wallet w
                    INNER JOIN user u ON u.id = w.userid
                ";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(WalletReaderMap(reader));
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
        public async Task<Wallet?> SelectByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT w.id AS wallet_id, w.balance, u.id AS user_id, u.username, u.passhash, u.passsalt, u.email, u.availablecoins
                    FROM wallet w
                    INNER JOIN user u ON u.id = w.id
                    WHERE w.id = @id;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return WalletReaderMap(reader);
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

        public async Task<Wallet> InsertAsync(Wallet wallet)
        {
            if (wallet == null)
                throw new ArgumentNullException(nameof(wallet));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    INSERT INTO wallet(userid, balance)
                    VALUES(@userid, @balance);
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = wallet.User.Id;
                cmd.Parameters.Add("@balance", MySqlDbType.Decimal).Value = wallet.Balance;
                
                var result = await cmd.ExecuteScalarAsync();
                int id = Convert.ToInt32(result);
                wallet.Id = id;

                return wallet;
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
        public async Task<int> UpdateAsync(int id, Wallet wallet)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            if (wallet == null)
                throw new ArgumentNullException(nameof(wallet));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    UPDATE wallet SET 
                        userid = @userid, 
                        balance = @balance 
                    WHERE id = @id;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = wallet.User.Id;
                cmd.Parameters.Add("@balance", MySqlDbType.Decimal).Value = wallet.Balance;
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
        public async Task<Wallet?> SelectByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT w.id AS wallet_id, w.balance, u.id AS user_id, u.username, u.passhash, u.passsalt, u.email, u.availablecoins
                    FROM wallet w
                    INNER JOIN user u ON u.id = w.userid
                    WHERE w.userid = @userid;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@userid", MySqlDbType.Int32).Value = userId;
                using var reader = await cmd.ExecuteReaderAsync();
                
                var result = await reader.ReadAsync() ? WalletReaderMap(reader) : null;
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
        public async Task<int> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    DELETE FROM wallet 
                    WHERE id = @id;
                ";

                using var cmd = new MySqlCommand(query, conn);
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

        private Wallet WalletReaderMap(MySqlDataReader reader)
        {
            return new Wallet(
                reader.GetInt32("wallet_id"),
                UserReaderMap(reader),
                reader.GetDecimal("balance")
            );
        }
        private User UserReaderMap(MySqlDataReader reader)
        {
            return new User(
                reader.GetInt32("user_id"),
                reader.GetString("username"),
                reader.GetString("passhash"),
                reader.GetString("passsalt"),
                reader.GetString("email"),
                reader.GetByte("availablecoins")
            );
        }
    }
}
