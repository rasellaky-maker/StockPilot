using BLL.CoreServisecs;
using BLL.Logger;
using BO.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SEP_WebApplication.Attributes;
using SEP_WebApplication.Infrastructure;

namespace SEP_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinController : ControllerBase
    {
        private readonly CoinService _coinService;
        private readonly UserCoinService _userCoinService;
        private readonly CoinPriceHistoryService _coinPriceHistoryService;
        private readonly UserService _userService;

        public CoinController(
            CoinService coinService,
            UserCoinService userCoinService,
            CoinPriceHistoryService coinPriceHistoryService,
            UserService userService)
        {
            _coinService = coinService;
            _userCoinService = userCoinService;
            _coinPriceHistoryService = coinPriceHistoryService;
            _userService = userService;
        }

        private async Task<ObjectResult> DatabaseUnavailable(Exception ex, string context)
        {
            await Log.WriteLine($"{context}: {ex}", "ERROR");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Database service is unavailable.");
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Coin>>> Get()
        {
            try
            {
                var coins = await DbRetry.RunAsync(() => _coinService.GetAsync());
                return Ok(coins);
            }
            catch (Exception ex) when (ex is MySqlException or InvalidOperationException)
            {
                return await DatabaseUnavailable(ex, "GetAll failed");
            }
        }

        [HttpGet("id/{id:int}")]
        public async Task<ActionResult<Coin>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Id must be greater than 0.");

            try
            {
                var coin = await DbRetry.RunAsync(() => _coinService.GetAsync(id));

                if (coin is null)
                    return NotFound($"Coin with id {id} was not found.");

                return Ok(coin);
            }
            catch (Exception ex) when (ex is MySqlException or InvalidOperationException)
            {
                return await DatabaseUnavailable(ex, $"GetById failed (coin {id})");
            }
        }

        [HttpGet("search/{name}")]
        public async Task<ActionResult<List<Coin>>> Search(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return await Get();

            try
            {
                var search = await DbRetry.RunAsync(() => _coinService.SearchAsync(name));
                return Ok(search);
            }
            catch (Exception ex) when (ex is MySqlException or InvalidOperationException)
            {
                return await DatabaseUnavailable(ex, $"Search failed ('{name}')");
            }
        }

        [HttpGet("{id:int}/history")]
        public async Task<ActionResult<List<PricePoint>>> GetHistory(int id, [FromQuery] string range = "1h")
        {
            if (id <= 0)
                return BadRequest("Id must be greater than 0.");

            try
            {
                var data = await DbRetry.RunAsync(() => _coinPriceHistoryService.GetHistoryAsync(id, range));
                return Ok(data);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) when (ex is MySqlException or InvalidOperationException)
            {
                await Log.WriteLine($"History failed (coin {id}, range {range}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Preishistorie ist momentan nicht verfügbar.");
            }
        }

        [HttpPost("create")]
        [ApiKey]
        public async Task<ActionResult<Coin>> Create([FromQuery(Name = "userId")] int userId, [FromBody] Coin coin)
        {
            if (userId <= 0)
                return BadRequest("User id must be greater than 0.");

            if (coin is null || string.IsNullOrWhiteSpace(coin.Name))
                return BadRequest("Name cannot be empty.");

            if (coin.Value <= 0)
                return BadRequest("Value must be greater than 0.");

            var name = coin.Name.Trim();

            try
            {
                // Check -> reserve -> create must not run twice in parallel for the same user.
                using var userLock = await UserLock.AcquireAsync(userId, HttpContext.RequestAborted);

                var user = await _userService.GetAsync(userId);
                if (user == null)
                {
                    await Log.WriteLine($"Create coin failed: user {userId} not found", "WARN");
                    return NotFound($"User with id {userId} was not found.");
                }

                if (user.AvailableCoins <= 0)
                {
                    await Log.WriteLine($"Create coin denied: user {userId} has no available coins", "WARN");
                    return BadRequest("User has no available coins.");
                }

                if (await _coinService.GetAsync(name) != null)
                {
                    await Log.WriteLine($"Create coin failed: name '{name}' already exists (user {userId})", "WARN");
                    return BadRequest($"A coin with the name '{name}' already exists.");
                }

                // Reserve the "coin slot" first: if creating the coin fails, it is given back.
                user.AvailableCoins--;
                var reserved = await _userService.Update(userId, user);
                if (reserved <= 0)
                {
                    await Log.WriteLine($"Create coin failed: could not update user {userId}", "ERROR");
                    return StatusCode(StatusCodes.Status500InternalServerError, "The user could not be updated.");
                }

                Coin? result;
                try
                {
                    result = await _coinService.CreateAsync(name, coin.Value.ToString(), coin.Image, userId);
                }
                catch
                {
                    await RestoreAvailableCoinAsync(userId);
                    throw;
                }

                if (result == null || result.Id <= 0)
                {
                    await RestoreAvailableCoinAsync(userId);
                    await Log.WriteLine($"Create coin failed: service returned no result (user {userId}, name '{name}')", "ERROR");
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed to create the coin.");
                }

                await Log.WriteLine($"Coin created: Id={result.Id}, Name='{result.Name}', Value={result.Value}, CreatedByUserId={userId}, AvailableCoinsLeft={user.AvailableCoins}");
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                await Log.WriteLine($"Create coin bad input (user {userId}): {ex.Message}", "WARN");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Create coin service unavailable (user {userId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "The service is temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Create coin unexpected error (user {userId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        private async Task RestoreAvailableCoinAsync(int userId)
        {
            try
            {
                var current = await _userService.GetAsync(userId);
                if (current is null)
                    return;

                current.AvailableCoins++;
                await _userService.Update(userId, current);
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"INCONSISTENT STATE: could not give back the available coin of user {userId}: {ex}", "ERROR");
            }
        }

        [HttpPost("buy")]
        [ApiKey]
        public async Task<ActionResult<bool>> Buy([FromQuery] int userId, [FromQuery] int coinId, [FromQuery] decimal quantity = 1)
        {
            if (userId <= 0 || coinId <= 0)
                return BadRequest("userId and coinId must be greater than 0.");

            if (quantity <= 0)
                return BadRequest("Quantity must be greater than 0.");

            try
            {
                var result = await _userCoinService.BuyAsync(userId, coinId, quantity);
                if (!result)
                {
                    await Log.WriteLine($"Buy failed: user {userId}, coin {coinId}, quantity {quantity} (coin/wallet not found or insufficient funds)", "WARN");
                    return BadRequest("Purchase failed: coin or wallet not found, or insufficient funds.");
                }

                await Log.WriteLine($"Buy succeeded: user {userId}, coin {coinId}, quantity {quantity}");
                return Ok(true);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await Log.WriteLine($"Buy bad input (user {userId}, coin {coinId}, quantity {quantity}): {ex.Message}", "WARN");
                return BadRequest(ex.Message);
            }
            catch (OwnCoinPurchaseException ex)
            {
                await Log.WriteLine($"Buy denied: user {userId} tried to buy own coin {coinId}", "WARN");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Buy service unavailable (user {userId}, coin {coinId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "The service is temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Buy unexpected error (user {userId}, coin {coinId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPost("sell")]
        [ApiKey]
        public async Task<ActionResult<decimal>> Sell([FromQuery] int userId, [FromQuery] int coinId, [FromQuery] decimal quantity = 1)
        {
            if (userId <= 0 || coinId <= 0)
                return BadRequest("userId and coinId must be greater than 0.");

            if (quantity <= 0)
                return BadRequest("Quantity must be greater than 0.");

            try
            {
                var revenue = await _userCoinService.SellAsync(userId, coinId, quantity);
                if (revenue is null)
                {
                    await Log.WriteLine($"Sell failed: user {userId}, coin {coinId}, quantity {quantity} (not enough coins, or coin/wallet not found)", "WARN");
                    return BadRequest("Sale failed: you do not own enough of this coin, or the coin or wallet was not found.");
                }

                await Log.WriteLine($"Sell succeeded: user {userId}, coin {coinId}, quantity {quantity}, revenue {revenue}");
                return Ok(revenue.Value);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await Log.WriteLine($"Sell bad input (user {userId}, coin {coinId}, quantity {quantity}): {ex.Message}", "WARN");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Sell service unavailable (user {userId}, coin {coinId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "The service is temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Sell unexpected error (user {userId}, coin {coinId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("user/{userId:int}")]
        [ApiKey]
        public async Task<ActionResult<List<UsersCoins>>> GetUserCoins(int userId)
        {
            if (userId <= 0)
            {
                await Log.WriteLine($"GetUserCoins bad input: invalid userId {userId}", "WARN");
                return BadRequest("Id must be greater than 0.");
            }

            try
            {
                var result = await DbRetry.RunAsync(() => _userCoinService.ShowCoins(userId));
                await Log.WriteLine($"GetUserCoins: user {userId}, {result.Count} entries returned");
                return Ok(result);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                await Log.WriteLine($"GetUserCoins bad input (user {userId}): {ex.Message}", "WARN");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"GetUserCoins service unavailable (user {userId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "The service is temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"GetUserCoins unexpected error (user {userId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("created-by/{userId}")]
        public async Task<IActionResult> GetCreatedCoins(int userId)
        {
            if (userId <= 0)
                return BadRequest("Invalid user ID.");

            try
            {
                var coins = await DbRetry.RunAsync(
                    () => _userCoinService.ShowCreatedCoins(userId)
                );

                return Ok(coins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
