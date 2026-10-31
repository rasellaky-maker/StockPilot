using BO.Models;
using MySqlConnector;

namespace DAL
{
    public class UserRepository
    {
        private string _connectionString = string.Empty;
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<User>> SelectAsync()
        {
            var result = new List<User>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                SELECT id, username, passhash, passsalt, email, availablecoins
                FROM user;
                ";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(UserReaderMap(reader));
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
        public async Task<User?> SelectByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT id, username, passhash, passsalt, email, availablecoins
                    FROM user
                    WHERE id = @id;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = id;
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return UserReaderMap(reader);
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
        public async Task<User?> SelectByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT id, username, passhash, passsalt, email, availablecoins
                    FROM user
                    WHERE username = @username;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@username", MySqlDbType.VarChar, 255).Value = username;
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return UserReaderMap(reader);
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
        public async Task<User?> SelectByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    SELECT id, username, passhash, passsalt, email, availablecoins
                    FROM user
                    WHERE email = @email;
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@email", MySqlDbType.VarChar, 255).Value = email;
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    return UserReaderMap(reader);

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
        public async Task<User> InsertAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
                    INSERT INTO user(username, passhash, passsalt, email, availablecoins)
                    VALUES(@username, @passhash, @passsalt, @email, @availablecoins);
                    SELECT LAST_INSERT_ID();
                ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@username", MySqlDbType.VarChar, 255).Value = user.Username;
                cmd.Parameters.Add("@passhash", MySqlDbType.VarChar, 255).Value = user.PassHash;
                cmd.Parameters.Add("@passsalt", MySqlDbType.VarChar, 255).Value = user.PassSalt;
                cmd.Parameters.Add("@email", MySqlDbType.VarChar, 255).Value = user.Email;
                cmd.Parameters.Add("@availablecoins", MySqlDbType.Byte).Value = user.AvailableCoins;

                var result = await cmd.ExecuteScalarAsync();
                user.Id = Convert.ToInt32(result);

                return user;
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
        public async Task<int> UpdateAsync(int id, User user)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                await conn.OpenAsync();

                const string query = @"
            UPDATE user SET 
                username = @username, 
                passhash = @passhash, 
                passsalt = @passsalt, 
                email = @email, 
                availablecoins = @availablecoins
            WHERE id = @id;
            ";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.Add("@username", MySqlDbType.VarChar, 255).Value = user.Username;
                cmd.Parameters.Add("@passhash", MySqlDbType.VarChar, 255).Value = user.PassHash;
                cmd.Parameters.Add("@passsalt", MySqlDbType.VarChar, 255).Value = user.PassSalt;
                cmd.Parameters.Add("@email", MySqlDbType.VarChar, 255).Value = user.Email;
                cmd.Parameters.Add("@availablecoins", MySqlDbType.Byte).Value = user.AvailableCoins;
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
                    DELETE FROM user 
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

        private User UserReaderMap(MySqlDataReader reader)
        {
            return new User(
                reader.GetInt32("id"),
                reader.GetString("username"),
                reader.GetString("passhash"),
                reader.GetString("passsalt"),
                reader.GetString("email"),
                reader.GetByte("availablecoins")
            );
        }
    }
}