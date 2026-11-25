using BO.Models;
using Microsoft.VisualBasic.Logging;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using view.Mapps;

namespace view.HttpSepClient
{
    public class SepClient
    {
        private HttpClient client = new HttpClient();

        internal SepClient(string baseUrl)
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.Add("X-Api-Key", "348thGFubg7345uff84BIBJJG%fjdgndjgu");
        }
        internal async Task<(HttpStatusCode Status, Coin? Coin, string? Error)> HttpBuy(int coinId, int userId, decimal quantity)
        {
            if (coinId <= 0)
                throw new ArgumentOutOfRangeException(nameof(coinId));

            try
            {
                // the server reads these values from the query string ([FromQuery])
                string query = $"userId={userId}&coinId={coinId}&quantity={quantity.ToString(CultureInfo.InvariantCulture)}";

                using var response = await client.PostAsync($"api/Coin/buy?{query}", null);

                if (response.IsSuccessStatusCode)
                {
                    Coin? coin = null;
                    try
                    {
                        coin = await response.Content.ReadFromJsonAsync<Coin>();
                    }
                    catch (JsonException)
                    {
                        coin = null;
                    }

                    return (response.StatusCode, coin, null);
                }

                var error = await response.Content.ReadAsStringAsync();
                return (response.StatusCode, null, error);
            }
            catch (HttpRequestException)
            {
                return (HttpStatusCode.ServiceUnavailable, null, LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null, LocalMapps.TimeoutError());
            }
        }
        internal async Task<(HttpStatusCode Status, User? User, string? Error)> HttpLogn(User user)
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

                    return (response.StatusCode, loggedUser, null);
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
                return (HttpStatusCode.ServiceUnavailable, null, LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null, LocalMapps.TimeoutError());
            }
            catch (JsonException)
            {
                return (HttpStatusCode.InternalServerError, null, LocalMapps.InvalidFormatError());
            }
        }
        internal async Task<(HttpStatusCode Status, User? User, string? Error)> HttpSingIn(User user)
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
                {
                    var createdUser = JsonSerializer.Deserialize<User>(
                        result,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );

                    if (createdUser == null)
                    {
                        return (response.StatusCode, null, "User response is empty.");
                    }

                    return (response.StatusCode, createdUser, null);
                }

                return (response.StatusCode, null, result);
            }
            catch (HttpRequestException)
            {
                return (HttpStatusCode.ServiceUnavailable, null, LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null, LocalMapps.TimeoutError());
            }
            catch (JsonException)
            {
                return (HttpStatusCode.InternalServerError, null, LocalMapps.InvalidFormatError());
            }
        }
        internal async Task<(HttpStatusCode Status, List<Coin>? Coins, string? Error)> HttpGetAll()
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
                return (HttpStatusCode.ServiceUnavailable, null, LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null, LocalMapps.TimeoutError());
            }
            catch (JsonException)
            {
                return (HttpStatusCode.InternalServerError, null, LocalMapps.InvalidFormatError());
            }
        }
        internal async Task<(HttpStatusCode Status, Coin? Coin, string? Error)> HttpGetById(int id)
        {
            try
            {
                using var response = await client.GetAsync($"api/Coin/id/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var coin = await response.Content.ReadFromJsonAsync<Coin>();

                    return (response.StatusCode, coin, null);
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
                return (HttpStatusCode.ServiceUnavailable, null, LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null, LocalMapps.TimeoutError());
            }
            catch (JsonException)
            {
                return (HttpStatusCode.InternalServerError, null, LocalMapps.InvalidFormatError());
            }
        }
        internal async Task<(HttpStatusCode Status, List<Coin>? Coin, string? Error)> HttpSearch(string searchTerm)
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

                    return (response.StatusCode, coins, null);
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
                return (HttpStatusCode.ServiceUnavailable, null, LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null, LocalMapps.TimeoutError());
            }
            catch (JsonException)
            {
                return (HttpStatusCode.InternalServerError, null, LocalMapps.InvalidFormatError());
            }
        }
        internal async Task<(HttpStatusCode Status, Wallet? Wallet, string? Error)> HttpGetWallet(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

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
                return (HttpStatusCode.ServiceUnavailable, null, LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null, LocalMapps.TimeoutError());
            }
            catch (JsonException)
            {
                return (HttpStatusCode.InternalServerError, null, LocalMapps.InvalidFormatError());
            }
        }
        internal async Task<(HttpStatusCode Status, decimal SalePreis, string? Error)> HttpSaleCoin(int userId, int coinId, decimal quantity = 1)
        {
            if (coinId <= 0)
                return (HttpStatusCode.BadRequest, 0, "Invalid coin id");

            if (quantity <= 0)
                return (HttpStatusCode.BadRequest, 0, "Quantity must be greater than 0");

            if (userId <= 0)
                return (HttpStatusCode.BadRequest, 0, "Invalid user id");

            try
            {
                // the server route is api/Coin/sell and it reads the values from the query string
                string query = $"userId={userId}&coinId={coinId}&quantity={quantity.ToString(CultureInfo.InvariantCulture)}";

                using var response = await client.PostAsync($"api/Coin/sell?{query}", null);

                if (!response.IsSuccessStatusCode)
                {
                    return (
                        response.StatusCode,
                        0,
                        await response.Content.ReadAsStringAsync());
                }

                var salePrice = await response.Content.ReadFromJsonAsync<decimal>();

                return (HttpStatusCode.OK, salePrice, null);
            }
            catch (Exception ex)
            {
                return (
                    HttpStatusCode.InternalServerError,
                    0,
                    ex.Message
                );
            }

        }

        internal async Task<(HttpStatusCode Status, List<PricePoint>? Points, string? Error)>
            HttpGetHistory(int coinId, string range = "1h")
        {
            if (coinId <= 0)
                return (HttpStatusCode.BadRequest, null, "Invalid coin ID.");

            try
            {
                using var response = await client.GetAsync(
                    $"api/Coin/{coinId}/history?range={Uri.EscapeDataString(range)}"
                );

                if (response.IsSuccessStatusCode)
                {
                    var points = await response.Content.ReadFromJsonAsync<List<PricePoint>>();

                    return (response.StatusCode, points ?? new List<PricePoint>(), null);
                }

                var error = await response.Content.ReadAsStringAsync();

                return (response.StatusCode, null, error);
            }
            catch (HttpRequestException)
            {
                return (HttpStatusCode.ServiceUnavailable, null,
                    LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null,
                    LocalMapps.TimeoutError());
            }
            catch (JsonException)
            {
                return (HttpStatusCode.InternalServerError, null,
                    LocalMapps.InvalidFormatError());
            }
        }

        internal async Task<(HttpStatusCode Status, Coin? Coin, string? Error)> HttpCreateCoin(int userId, Coin coin)
        {
            if (userId <= 0)
                return (HttpStatusCode.BadRequest, null, "Invalid user id.");

            try
            {
                using var response = await client.PostAsJsonAsync($"api/Coin/create?userId={userId}", coin);

                if (response.IsSuccessStatusCode)
                {
                    var created = await response.Content.ReadFromJsonAsync<Coin>();
                    return (response.StatusCode, created, null);
                }

                var error = await response.Content.ReadAsStringAsync();
                return (response.StatusCode, null, error);
            }
            catch (HttpRequestException)
            {
                return (HttpStatusCode.ServiceUnavailable, null, LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null, LocalMapps.TimeoutError());
            }
            catch (JsonException)
            {
                return (HttpStatusCode.InternalServerError, null, LocalMapps.InvalidFormatError());
            }
        }

        internal async Task<(HttpStatusCode Status, List<Coin>? Coins, string? Error)> HttpGetCreatedCoins(int userId)
        {
            if (userId <= 0)
                return (HttpStatusCode.BadRequest, null, "Invalid user id.");

            try
            {
                using var response = await client.GetAsync($"api/Coin/created-by/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var coins = await response.Content.ReadFromJsonAsync<List<Coin>>();
                    return (response.StatusCode, coins ?? new List<Coin>(), null);
                }

                var error = await response.Content.ReadAsStringAsync();
                return (response.StatusCode, null, error);
            }
            catch (HttpRequestException)
            {
                return (HttpStatusCode.ServiceUnavailable, null, LocalMapps.ConnectionError());
            }
            catch (TaskCanceledException)
            {
                return (HttpStatusCode.RequestTimeout, null, LocalMapps.TimeoutError());
            }
            catch (JsonException)
            {
                return (HttpStatusCode.InternalServerError, null, LocalMapps.InvalidFormatError());
            }
        }

    }
}