using BLL.CoreServisecs;
using BLL.Logger;
using BO.Models;
using Microsoft.AspNetCore.Mvc;
using MyApi.Attributes;
using SEP_WebApplication.Attributes;
using System.Reflection.Metadata;
using System.Security.Cryptography;

namespace SEP_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService = new UserService(MainConnnection.Connection);

        private readonly WalletService _walletService = new WalletService(MainConnnection.Connection);

        [HttpPost("register")]
        [ApiKey]
        public async Task<ActionResult<User>> Register([FromForm] string username, [FromForm] string password, [FromForm] string email)
        {
            try
            {
                var result = await _userService.SignIn(username, password, email);

                if (result == null || result.Id <= 0)
                {
                    await Log.WriteLine($"Register failed: service returned no user (username '{username}')", "ERROR");
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                await _walletService.CreateAsync(new Wallet(0, result, 0));

                await Log.WriteLine($"User registered: Id={result.Id}, username '{result.Username}'");
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (ArgumentException ex)
            {
                await Log.WriteLine($"Register bad input (username '{username}'): {ex.Message}", "WARN");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Register service unavailable (username '{username}'): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Register unexpected error (username '{username}'): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPost("login")]
        [ApiKey]
        public async Task<ActionResult<User>> Login([FromForm] string username, [FromForm] string password)
        {
            try
            {
                var result = await _userService.LogIn(username, password);
                if (!result)
                {
                    await Log.WriteLine($"Login failed: invalid credentials for username '{username}'", "WARN");
                    return Unauthorized("Invalid username or password.");
                }

                var user = await _userService.GetByUsername(username);
                await Log.WriteLine($"Login succeeded: username '{username}'");
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"Login service unavailable (username '{username}'): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Login unexpected error (username '{username}'): {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("loggs")]
        [AdminApiKey]
        public async Task<ActionResult<string>> GetLoggs()
        {
            return await Log.ReadText();
        }

        [HttpDelete("{id}")]
        [AdminApiKey]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                await Log.WriteLine($"User Delete({id}): rejected, id must be greater than 0.");
                return BadRequest("Id must be greater than 0.");
            }

            try
            {
                var deleted = await _userService.Delete(id);

                if (!deleted)
                {
                    await Log.WriteLine($"User Delete({id}): user not found.");
                    return NotFound($"User with id {id} not found.");
                }

                await Log.WriteLine($"User Delete({id}): user deleted successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"User Delete({id}) failed: {ex}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
            }
        }
    }
}
