using BLL.PassServices;
using BO.Models;
using DAL;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BLL.CoreServisecs
{
    public class UserService
    {
        private UserRepository _repo { get; set; } = null;
        public UserService(string connectionString)
        {
            _repo = new UserRepository(connectionString);
        }

        public async Task<List<User>> GetAsync()
        {
            return await _repo.SelectAsync();
        }

        public async Task<User?> GetByUsername(string username)
        {
            return await _repo.SelectByUsername(username);
        }

        public async Task<User?> GetAsync(int id)
        {
            return await _repo.SelectByIdAsync(id);
        }

        public async Task<int> Update(int id, User user)
        {
            return await _repo.UpdateAsync(id, user);   
        }

        public async Task<bool> LogIn(string username, string password)
        {
            var error = ChackLogInData(username, password);

            if (!string.IsNullOrWhiteSpace(error))
                return false;

            var user = await _repo.SelectByUsername(username);

            if (user == null)
                return false;

            return PasswordHasher.VerifyPassword(password, user.PassHash, user.PassSalt);
        }

        public async Task<User> SignIn(string username, string password, string email)
        {
            string checkMessage = await ChackSignInData(username, password, email);

            if (!string.IsNullOrEmpty(checkMessage))
                throw new ArgumentException(checkMessage);

            PasswordHasher.HashPassword(password, out string hash, out string salt);
            
            return await _repo.InsertAsync(new User(0, username, hash, salt, email, 1));
        }

        public async Task<string> ChackSignInData(string username, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "Bitte geben Sie einen Benutzernamen ein.";

            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return "Das Passwort muss mindestens 6 Zeichen lang sein.";

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.Length < 3)
                return "Bitte geben Sie eine gültige E-Mail-Adresse ein.";

            var user = await _repo.SelectByUsername(username);
            if (user != null)
                return $"Der Benutzername mit '{username}' username ist bereits vergeben.";

            var emailUser = await _repo.SelectByEmail(email);
            if (emailUser != null)
                return $"Der Benutzername mit '{email}' email ist bereits vergeben.";

            return "";
        }
        public string ChackLogInData(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                return "Bitte geben Sie einen Benutzernamen ein.";

            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return "Das Passwort muss mindestens 6 Zeichen lang sein.";

            return "";
        }

        public async Task<bool> Delete(int id)
        {
            var result = await _repo.DeleteAsync(id);
            return result != 0;
        }
    }
}
