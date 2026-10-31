using BLL.CoreServisecs;
using BLL.Logger;
using BO.Models;
using Microsoft.AspNetCore.Mvc;
using MyApi.Attributes;
using MySqlConnector;
using SEP_WebApplication.Attributes;

namespace SEP_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinController : ControllerBase
    {
        private readonly CoinService _coinService = new CoinService(MainConnnection.Connection);
        internal static readonly UserCoinService _userCoinService = new UserCoinService(MainConnnection.Connection);
        private readonly CoinPriceHistoryService _coinPriceHistoryService = new CoinPriceHistoryService(MainConnnection.Connection);

        [HttpGet("GetAll")]
        public async Task<ActionResult<List<Coin>>> Get()
        {
            try
            {
                var coins = await _coinService.GetAsync();
                return Ok(coins);
            }
            catch (MySqlException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"GetAll failed: {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
        }

        [HttpGet("id:{id}")]
        public async Task<ActionResult<Coin?>> GetById(int id)
        {
            try
            {
                var coin = await _coinService.GetAsync(id);
                if (coin == null)
                    return NotFound(id);
                return Ok(coin);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest($"Invalid ID: {ex.Message}");
            }
            catch (MySqlException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"GetById failed (coin {id}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
        }

        [HttpGet("search/{name}")]
        public async Task<ActionResult<List<Coin>>> Search(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return await Get();

            try
            {
                var search = await _coinService.SearchAsync(name);
                return Ok(search);
            }
            catch (ArgumentNullException)
            {
                return await Get();
            }
            catch (MySqlException ex)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Search failed ('{name}'): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
        }

        [HttpGet("{id}/history")]
        public async Task<ActionResult<List<PricePoint>>> GetHistory(int id, [FromQuery] string range = "1h")
        {
            try
            {
                var data = await _coinPriceHistoryService.GetHistoryAsync(id, range);
                return Ok(data);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"History failed (coin {id}, range {range}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Preishistorie ist momentan nicht verfügbar.");
            }
        }

        [HttpPost("create")]
        [ApiKey]
        public async Task<ActionResult<Coin>> Create([FromQuery] int id, [FromBody] Coin coin)
        {
            try
            {
                var userService = new UserService(MainConnnection.Connection);

                var user = await userService.GetAsync(id);
                if (user == null)
                {
                    await Log.WriteLine($"Create coin failed: user {id} not found", "WARN");
                    return NotFound($"User with id {id} was not found.");
                }

                if (user.AvailableCoins <= 0)
                {
                    await Log.WriteLine($"Create coin denied: user {id} has no available coins", "WARN");
                    return BadRequest("User has no available coins.");
                }

                if (await _coinService.GetAsync(coin.Name) != null)
                {
                    await Log.WriteLine($"Create coin failed: name '{coin.Name}' already exists (user {id})", "WARN");
                    return BadRequest($"A coin with the name '{coin.Name}' already exists.");
                }

                var result = await _coinService.CreateAsync(coin.Name, coin.Value.ToString(), coin.Image, id);
                if (result == null || result.Id <= 0)
                {
                    await Log.WriteLine($"Create coin failed: service returned no result (user {id}, name '{coin.Name}')", "ERROR");
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, "Failed to create the coin.");
                }

                user.AvailableCoins--;

                var updateUserResult = await userService.Update(id, user);
                if (updateUserResult <= 0)
                {
                    await Log.WriteLine($"INCONSISTENT STATE: coin {result.Id} ('{result.Name}') created, but user {id} was not updated", "ERROR");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Coin was created, but the user could not be updated.");
                }

                await Log.WriteLine($"Coin created: Id={result.Id}, Name='{result.Name}', Value={result.Value}, CreatedByUserId={id}, AvailableCoinsLeft={user.AvailableCoins}");
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                await Log.WriteLine($"Create coin bad input (user {id}): {ex.Message}", "WARN");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Create coin service unavailable (user {id}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "The service is temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Create coin unexpected error (user {id}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPost("buy")]
        [ApiKey]
        public async Task<ActionResult<bool>> Buy(int userId, int coinId, decimal quantity = 1)
        {
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
        public async Task<ActionResult<decimal>> Sell(int userId, int coinId, decimal quantity = 1)
        {
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

        [HttpGet("user/{userId}")]
        [ApiKey]
        public async Task<ActionResult<List<UsersCoins>>> GetUserCoins(int userId)
        {
            if (userId <= 0)
            {
                await Log.WriteLine($"GetUserCoins bad input: invalid userId {userId}", "WARN");
                return BadRequest();
            }

            try
            {
                var result = await _userCoinService.ShowCoins(userId);
                await Log.WriteLine($"GetUserCoins: user {userId}, {result.Count} entries returned");
                return result;
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
    }
}