using BLL.CoreServisecs;
using BLL.Logger;
using BO.Models;
using Microsoft.AspNetCore.Mvc;
using MyApi.Attributes;
using SEP_WebApplication.Controllers;
using System.Reflection.Metadata;

namespace SEP_WebApplication.Attributes
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly WalletService _walletService = new WalletService(MainConnnection.Connection);

        [HttpGet]
        [AdminApiKey]
        public async Task<ActionResult<List<Wallet>>> Get()
        {
            try
            {
                var result = await _walletService.GetAsync();
                await Log.WriteLine($"Wallets listed by admin: {result.Count} entries returned");
                return result;
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

        [HttpPut("{id}")]
        [AdminApiKey]
        public async Task<ActionResult<int>> Update(int id, Wallet wallet)
        {
            try
            {
                var rows = await _walletService.UpdateAsync(id, wallet);

                if (rows <= 0)
                {
                    await Log.WriteLine($"Wallet update: no rows changed for wallet {id} (not found?)", "WARN");
                    return NotFound($"Wallet with id {id} was not found.");
                }

                await Log.WriteLine($"Wallet updated by admin: wallet {id}, userId={wallet.User?.Id}, new balance={wallet.Balance}");
                return rows;
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

        [HttpGet("{userId}")]
        [ApiKey]
        public async Task<ActionResult<Wallet>> GetByUserId(int userId)
        {
            if (userId <= 0)
                return BadRequest("Id must be greater than 0.");

            try
            {
                var result = await _walletService.GetByUserIdAsync(userId);

                if (result == null)
                    return NotFound($"Wallet with id {userId} not found.");

                result.UsersCoins = await CoinController._userCoinService.ShowCoins(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }
    }
}