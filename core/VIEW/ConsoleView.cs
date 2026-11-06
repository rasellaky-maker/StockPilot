using BO.Models;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace VIEW
{
    public class ConsoleView
    {
        public User localUser = new User();

        private const string baseUrl = "https://localhost:7132/";

        public CultureInfo culture = new CultureInfo("de-DE");

        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };

        public ConsoleView()
        {
            client.DefaultRequestHeaders.Add("X-Api-Key", "348thGFubg7345uff84BIBJJG%fjdgndjgu");
        }

        public string ShowMenu()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("-list");
            sb.AppendLine("-get");
            sb.AppendLine("-search");
            sb.AppendLine("-registrate");
            sb.AppendLine("-login");
            sb.AppendLine();
            sb.AppendLine("-mycoin");
            sb.AppendLine("-mywallet");
            sb.AppendLine("-buy");
            sb.AppendLine("-sale");
            sb.AppendLine("-track");
            sb.AppendLine();
            sb.AppendLine("-?");
            sb.AppendLine("-clear");
            sb.AppendLine("-user");
            sb.AppendLine("-exit");

            return sb.ToString();
        }
        public string UserInfo()
        {
            if (localUser.Id == 0)
                return "User is not registered.";
            return localUser.AllInfo();
        }
        public async Task<string> List()
        {
            var (status, coins, error) = await HttpGetAll();

            if (status == HttpStatusCode.OK && coins != null)
            {
                return $"{OutputMapp(coins)}Status: {status} {(int)status}";
            }

            return $"{error}\nStatus: {status} {(int)status}";
        }
        public async Task<string> Get()
        {
            Console.Write("ID: ");

            string id = Console.ReadLine() ?? "";

            if (!int.TryParse(id, out int iid))
                return "Invalid ID. Please enter a valid number.";

            var (status, coin, error) = await HttpGetById(iid);

            if (status == HttpStatusCode.OK && coin != null)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"Coin {id}:");
                sb.AppendLine(coin.ToString());
                sb.AppendLine($"Status: {status} {(int)status}");

                return sb.ToString();
            }

            return $"Unable to retrieve coin {id}.\n{error}\nStatus: {status} {(int)status}";
        }
        public async Task<string> Search()
        {
            Console.Write("Search name: ");

            string term = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(term))
                return "Please enter a search term.";

            var (status, coins, error) = await HttpSearch(term);

            if (status == HttpStatusCode.OK && coins != null)
            {
                return $"{OutputMapp(coins)}Status: {status} {(int)status}";
            }

            if (status == HttpStatusCode.NotFound)
                return "No results found. Try a different search.";

            return $"{error}\nStatus: {status} {(int)status}";
        }
        public async Task<string> SingIn()
        {
            Console.WriteLine("----- Registration -----");

            string email;
            string username;
            string password;
            string password2;

            do
            {
                Console.Write("E-Mail: ");
                email = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(email))
                    Console.WriteLine("Please enter your email address.");

            } while (string.IsNullOrWhiteSpace(email));

            do
            {
                Console.Write("Username: ");
                username = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(username))
                    Console.WriteLine("Please enter a username.");

            } while (string.IsNullOrWhiteSpace(username));

            do
            {
                password = ReadPassword();

                Console.Write("Repeat ");
                password2 = ReadPassword();

                if (string.IsNullOrWhiteSpace(password))
                    Console.WriteLine("Password cannot be empty.");

                else if (password != password2)
                    Console.WriteLine("Passwords do not match. Please try again.");

            } while (string.IsNullOrWhiteSpace(password) || password != password2);

            var user = new User(
                0,
                username,
                password,
                string.Empty,
                email,
                0
            );

            var (status, singinUser, error) = await HttpSingIn(user);

            if (status == HttpStatusCode.Created && singinUser != null)
            {
                localUser = singinUser;

                return "Registration successful!\nWelcome aboard!";
            }

            return error ?? "Registration failed. Please check your information and try again.";
        }
        public async Task<string> LogIn()
        {
            Console.WriteLine("----- Login -----");

            string username;
            string password;

            do
            {
                Console.Write("Username: ");
                username = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(username))
                    Console.WriteLine("Please enter your username.");

            } while (string.IsNullOrWhiteSpace(username));

            do
            {
                password = ReadPassword();

                if (string.IsNullOrWhiteSpace(password))
                    Console.WriteLine("Please enter your password.");

            } while (string.IsNullOrWhiteSpace(password));

            var user = new User(0, username, password, "", "", 0);

            var result = await HttpLogn(user);

            if (result.Status == HttpStatusCode.OK && result.User != null)
            {
                localUser = result.User;

                return $"Login successful!\nWelcome, {localUser.Username}!";
            }

            return result.Error ?? "Login failed. Please try again.";
        }
        public async Task<string> GetWallet()
        {
            if (localUser.Id == 0)
                return "❌ You are not logged in.";

            var result = await HttpGetWallet(localUser.Id);

            if (result.Status == HttpStatusCode.OK && result.Wallet != null)
            {
                var wallet = result.Wallet;
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("╔══════════════════════════════════╗");
                sb.AppendLine($"║       💰 {localUser.Username}'s WALLET");
                sb.AppendLine("╠══════════════════════════════════╣");
                sb.AppendLine($"║ Wallet ID: {wallet.Id}");
                sb.AppendLine($"║ Balance:   {wallet.Balance:N2}");
                sb.AppendLine("╠══════════════════════════════════╣");

                sb.AppendLine("║ 🪙 COINS");

                if (wallet.UsersCoins.Any())
                {
                    foreach (var coin in wallet.UsersCoins)
                    {
                        sb.AppendLine($"║ {coin}");
                    }
                }
                else
                {
                    sb.AppendLine("║ No coins in wallet.");
                }

                sb.AppendLine("╠══════════════════════════════════╣");
                sb.AppendLine($"║ Status: {result.Status} ({(int)result.Status})");
                sb.AppendLine("╚══════════════════════════════════╝");

                return sb.ToString();
            }

            return $"❌ {result.Error}\nStatus: {result.Status} ({(int)result.Status})";
        }

        //----Http

        private async Task<(HttpStatusCode Status, User? User, string? Error)> HttpLogn(User user)
        {
            try
            {
                var data = new Dictionary<string, string>
                {
                    ["username"] = user.Username,
                    ["password"] = user.PassHash
                };

                using var content = new FormUrlEncodedContent(data);

                using var response = await client.PostAsync(
                    "api/User/login",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var loggedUser = await response.Content.ReadFromJsonAsync<User>();

                    if (loggedUser == null)
                    {
                        return (
                            response.StatusCode,
                            null,
                            "Unable to retrieve your account information. Please try again."
                        );
                    }

                    return (
                        response.StatusCode,
                        loggedUser,
                        null
                    );
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (
                        response.StatusCode,
                        null,
                        "Invalid username or password. Please try again."
                    );
                }

                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    return (
                        response.StatusCode,
                        null,
                        "Login service is temporarily unavailable. Please try again later."
                    );
                }

                return (
                    response.StatusCode,
                    null,
                    "Something went wrong while signing in. Please try again later."
                );
            }
            catch (HttpRequestException)
            {
                return (
                    HttpStatusCode.ServiceUnavailable,
                    null,
                    ConnectionError()
                );
            }
            catch (TaskCanceledException)
            {
                return (
                    HttpStatusCode.RequestTimeout,
                    null,
                    TimeoutError()
                );
            }
        }
        private async Task<(HttpStatusCode Status, User? User, string? Error)> HttpSingIn(User user)
        {
            try
            {
                var data = new Dictionary<string, string>
                {
                    ["username"] = user.Username,
                    ["password"] = user.PassHash,
                    ["email"] = user.Email
                };

                using var content = new FormUrlEncodedContent(data);

                using var response = await client.PostAsync(
                    "api/User/register",
                    content
                );

                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return (response.StatusCode, user, null);

                return (
                    response.StatusCode,
                    null,
                    "Registration failed. Please check your information and try again."
                );
            }
            catch (HttpRequestException)
            {
                return (
                    HttpStatusCode.ServiceUnavailable,
                    null,
                    ConnectionError()
                );
            }
            catch (TaskCanceledException)
            {
                return (
                    HttpStatusCode.RequestTimeout,
                    null,
                    TimeoutError()
                );
            }
        }
        private async Task<(HttpStatusCode Status, List<Coin>? Coins, string? Error)> HttpGetAll()
        {
            try
            {
                using var response = await client.GetAsync("api/Coin/GetAll");

                if (response.IsSuccessStatusCode)
                {
                    var coins = await response.Content.ReadFromJsonAsync<List<Coin>>();

                    return (
                        response.StatusCode,
                        coins ?? new List<Coin>(),
                        null
                    );
                }

                return (
                    response.StatusCode,
                    null,
                    "Unable to retrieve coins. Please try again later."
                );
            }
            catch (HttpRequestException)
            {
                return (
                    HttpStatusCode.ServiceUnavailable,
                    null,
                    ConnectionError()
                );
            }
            catch (TaskCanceledException)
            {
                return (
                    HttpStatusCode.RequestTimeout,
                    null,
                    TimeoutError()
                );
            }
        }
        private async Task<(HttpStatusCode Status, Coin? Coin, string? Error)> HttpGetById(int id)
        {
            try
            {
                using var response = await client.GetAsync($"api/Coin/id:{id}");

                if (response.IsSuccessStatusCode)
                {
                    var coin = await response.Content.ReadFromJsonAsync<Coin>();

                    return (
                        response.StatusCode,
                        coin,
                        null
                    );
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return (
                        response.StatusCode,
                        null,
                        "Coin not found. Please check the ID."
                    );
                }

                return (
                    response.StatusCode,
                    null,
                    "Unable to retrieve this coin. Please try again later."
                );
            }
            catch (HttpRequestException)
            {
                return (
                    HttpStatusCode.ServiceUnavailable,
                    null,
                    ConnectionError()
                );
            }
            catch (TaskCanceledException)
            {
                return (
                    HttpStatusCode.RequestTimeout,
                    null,
                    TimeoutError()
                );
            }
        }
        private async Task<(HttpStatusCode Status, List<Coin>? Coin, string? Error)> HttpSearch(string searchTerm)
        {
            try
            {
                using var response = await client.GetAsync(
                    "api/Coin/search/" + Uri.EscapeDataString(searchTerm)
                );

                if (response.IsSuccessStatusCode)
                {
                    var coins = await response.Content.ReadFromJsonAsync<List<Coin>>();

                    if (coins == null || coins.Count == 0)
                    {
                        return (
                            HttpStatusCode.NotFound,
                            null,
                            "No results found."
                        );
                    }

                    return (
                        response.StatusCode,
                        coins,
                        null
                    );
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return (
                        response.StatusCode,
                        null,
                        "No results found. Try a different search."
                    );
                }

                return (
                    response.StatusCode,
                    null,
                    "Search failed. Please try again later."
                );
            }
            catch (HttpRequestException)
            {
                return (
                    HttpStatusCode.ServiceUnavailable,
                    null,
                    ConnectionError()
                );
            }
            catch (TaskCanceledException)
            {
                return (
                    HttpStatusCode.RequestTimeout,
                    null,
                    TimeoutError()
                );
            }
        }
        private async Task<(HttpStatusCode Status, Wallet? Wallet, string Error)> HttpGetWallet(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException(nameof(id));

            try
            {
                using var response = await client.GetAsync("api/Wallet/" + id);

                if (response.IsSuccessStatusCode)
                {
                    var wallet = await response.Content.ReadFromJsonAsync<Wallet>();

                    if (wallet == null)
                        return (
                            response.StatusCode,
                            null,
                            "Failed to retrieve wallet information."
                        );

                    return (response.StatusCode, wallet, null);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return (
                        response.StatusCode,
                        null,
                        $"Wallet with id {id} was not found."
                    );
                }

                return (
                    response.StatusCode,
                    null,
                    "Failed to retrieve wallet. Please try again later."
                );
            }
            catch (HttpRequestException)
            {
                return (
                    HttpStatusCode.ServiceUnavailable,
                    null,
                    ConnectionError()
                );
            }
            catch (TaskCanceledException)
            {
                return (
                    HttpStatusCode.RequestTimeout,
                    null,
                    TimeoutError()
                );
            }
        }

        // MAPS
        private string ConnectionError()
        {
            return "Unable to connect to the server.\n" +
                   "The server may be temporarily unavailable.\n" +
                   "Please try again later.";
        }
        private string TimeoutError()
        {
            return "The request took too long.\n" +
                   "Please try again later.";
        }
        private string OutputMapp(List<Coin> coins)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("+-----+----------------+----------------+-------+");
            sb.AppendLine("| id  |      Name      | Value          |Holders|");
            sb.AppendLine("+-----+----------------+----------------+-------+");

            foreach (var el in coins)
            {
                sb.AppendLine(
                    $"| {el.Id,-3} | {el.Name,-14} | {el.Value.ToString("N2", culture),-14} | {el.NumberOfBuyers,-5} |"
                );
            }

            sb.AppendLine("+-----+----------------+----------------+-------+");
            sb.AppendLine($"Total coins: {coins.Count}");

            return sb.ToString();
        }
        private string ReadPassword()
        {
            Console.Write("Password: ");

            string password = "";

            ConsoleKeyInfo key;

            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password = password[..^1];
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            }

            Console.WriteLine();

            return password;
        }
    }
}
