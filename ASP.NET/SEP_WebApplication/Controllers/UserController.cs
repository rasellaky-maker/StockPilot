using BLL.CoreServisecs;
using BLL.Logger;
using BO.Models;
using Microsoft.AspNetCore.Mvc;
using SEP_WebApplication.Attributes;

namespace SEP_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly WalletService _walletService;

        public UserController(UserService userService, WalletService walletService)
        {
            _userService = userService;
            _walletService = walletService;
        }

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
                    return StatusCode(StatusCodes.Status500InternalServerError, "The user could not be created.");
                }

                try
                {
                    await _walletService.CreateAsync(new Wallet(0, result, 0));
                }
                catch (Exception walletEx)
                {
                    // A user without a wallet cannot trade: undo the registration instead of leaving a half-created user.
                    await Log.WriteLine($"Register: wallet creation failed for user {result.Id}, rolling back: {walletEx}", "ERROR");
                    try
                    {
                        await _userService.Delete(result.Id);
                    }
                    catch (Exception deleteEx)
                    {
                        await Log.WriteLine($"INCONSISTENT STATE: user {result.Id} has no wallet and could not be deleted: {deleteEx}", "ERROR");
                    }

                    throw;
                }

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
                var loggedIn = await _userService.LogIn(username, password);
                if (!loggedIn)
                {
                    await Log.WriteLine($"Login failed: invalid credentials for username '{username}'", "WARN");
                    return Unauthorized("Invalid username or password.");
                }

                var user = await _userService.GetByUsername(username);
                if (user is null)
                {
                    // user was deleted between the two calls
                    await Log.WriteLine($"Login failed: user '{username}' disappeared after verification", "WARN");
                    return Unauthorized("Invalid username or password.");
                }

                await Log.WriteLine($"Login succeeded: username '{username}'");
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                await Log.WriteLine($"Login bad input (username '{username}'): {ex.Message}", "WARN");
                return BadRequest(ex.Message);
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
            try
            {
                return await Log.ReadText();
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }
            catch (DirectoryNotFoundException)
            {
                return string.Empty;
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"Reading the log failed: {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "The log could not be read.");
            }
        }

        [HttpDelete("{id:int}")]
        [AdminApiKey]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                await Log.WriteLine($"User Delete({id}): rejected, id must be greater than 0.", "WARN");
                return BadRequest("Id must be greater than 0.");
            }

            try
            {
                var deleted = await _userService.Delete(id);

                if (!deleted)
                {
                    await Log.WriteLine($"User Delete({id}): user not found.", "WARN");
                    return NotFound($"User with id {id} not found.");
                }

                await Log.WriteLine($"User Delete({id}): user deleted successfully.");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                await Log.WriteLine($"User Delete({id}) failed: {ex}", "ERROR");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Service temporarily unavailable.");
            }
            catch (Exception ex)
            {
                await Log.WriteLine($"User Delete({id}) failed: {ex}", "ERROR");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
