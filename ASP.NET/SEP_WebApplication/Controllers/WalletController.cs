using BLL.CoreServisecs;
using BLL.Logger;
using BO.Models;
using Microsoft.AspNetCore.Mvc;
using SEP_WebApplication.Attributes;
using SEP_WebApplication.Infrastructure;

namespace SEP_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly WalletService _walletService;
        private readonly UserCoinService _userCoinService;

        public WalletController(WalletService walletService, UserCoinService userCoinService)
        {
            _walletService = walletService;
            _userCoinService = userCoinService;
        }

        [HttpGet]
        [AdminApiKey]
        public async Task<ActionResult<List<Wallet>>> Get()
        {
            try
            {
                var result = await DbRetry.RunAsync(() => _walletService.GetAsync());
                await Log.WriteLine($"Wallets listed by admin: {result.Count} entries returned");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Wallet list service unavailable: {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "The service is temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Wallet list unexpected error: {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPut("{id:int}")]
        [AdminApiKey]
        public async Task<ActionResult<int>> Update(int id, [FromBody] Wallet wallet)
        {
            if (id <= 0)
                return BadRequest("Id must be greater than 0.");

            if (wallet is null || wallet.User is null || wallet.User.Id <= 0)
                return BadRequest("The wallet must contain a user with a valid id.");

            if (wallet.Balance < 0)
                return BadRequest("Balance must not be negative.");

            try
            {
                var rows = await _walletService.UpdateAsync(id, wallet);

                if (rows <= 0)
                {
                    await Log.WriteLine($"Wallet update: no rows changed for wallet {id} (not found?)", "WARN");
                    return NotFound($"Wallet with id {id} was not found.");
                }

                await Log.WriteLine($"Wallet updated by admin: wallet {id}, userId={wallet.User.Id}, new balance={wallet.Balance}");
                return Ok(rows);
            }
            catch (ArgumentException ex)
            {
                await Log.WriteLine($"Wallet update bad input (wallet {id}): {ex.Message}", "WARN");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Wallet update service unavailable (wallet {id}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "The service is temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Wallet update unexpected error (wallet {id}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("{userId:int}")]
        [ApiKey]
        public async Task<ActionResult<Wallet>> GetByUserId(int userId)
        {
            if (userId <= 0)
                return BadRequest("Id must be greater than 0.");

            try
            {
                var result = await DbRetry.RunAsync(() => _walletService.GetByUserIdAsync(userId));

                if (result == null)
                    return NotFound($"Wallet of user {userId} not found.");

                result.UsersCoins = await DbRetry.RunAsync(() => _userCoinService.ShowCoins(userId));
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Wallet get service unavailable (user {userId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "The service is temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Wallet get unexpected error (user {userId}): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
