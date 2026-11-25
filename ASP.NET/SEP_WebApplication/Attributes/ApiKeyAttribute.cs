using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SEP_WebApplication.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private readonly string[] _configKeys;

    public ApiKeyAttribute() : this("ApiKey", "AdminApiKey") { }

    public ApiKeyAttribute(params string[] configKeys) => _configKeys = configKeys;

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var config = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

        var expectedKeys = _configKeys
            .Select(name => config[name])
            .Where(key => !string.IsNullOrEmpty(key))
            .Select(key => key!)
            .ToArray();

        // Keys are not configured on the server: that is a server problem, not the caller's fault.
        if (expectedKeys.Length == 0)
        {
            var logger = ctx.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>().CreateLogger<ApiKeyAttribute>();
            logger.LogError("None of the configuration keys [{Keys}] is set - rejecting request.",
                string.Join(", ", _configKeys));
            ctx.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError);
            return;
        }

        if (!ctx.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var provided)
            || string.IsNullOrEmpty(provided.ToString()))
        {
            ctx.Result = new UnauthorizedResult();
            return;
        }

        var providedValue = provided.ToString();
        var ok = false;
        foreach (var expected in expectedKeys)
        {
            // No short-circuit: every key is compared, so timing does not reveal which one matched.
            if (SafeEquals(providedValue, expected))
                ok = true;
        }

        if (!ok)
        {
            ctx.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }

    private static bool SafeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}

public class AdminApiKeyAttribute : ApiKeyAttribute
{
    public AdminApiKeyAttribute() : base("AdminApiKey") { }
}
