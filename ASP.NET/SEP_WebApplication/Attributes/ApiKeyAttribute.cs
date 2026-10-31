using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyApi.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private readonly string[] _configKeys;
    public ApiKeyAttribute() : this("ApiKey", "AdminApiKey") { }

    public ApiKeyAttribute(params string[] configKeys) => _configKeys = configKeys;

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var config = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

        if (!ctx.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var provided))
        {
            ctx.Result = new UnauthorizedResult();
            return;
        }

        var ok = false;
        foreach (var name in _configKeys)
        {
            var expected = config[name];
            if (!string.IsNullOrEmpty(expected) && SafeEquals(provided.ToString(), expected))
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