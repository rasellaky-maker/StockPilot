using System.Globalization;
using System.Text.Json.Serialization.Metadata;
using BLL.CoreServisecs;
using BLL.Logger;
using BO.Models;
using Microsoft.AspNetCore.Diagnostics;
using MySqlConnector;
using SEP_WebApplication.Infrastructure;

namespace SEP_WebApplication
{
    public class Program
    {
        private const string ConnectionName = "GlobaleConnection";

        public static void Main(string[] args)
        {
            // One culture for the whole app: decimals are always "1.5", never "1,5",
            // regardless of the server's regional settings (BLL parses/prints values).
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Upsun / Platform.sh: home directory is read-only, use a writable temp directory.
            if (OperatingSystem.IsLinux() &&
                Environment.GetEnvironmentVariable("PLATFORM_APPLICATION_NAME") is not null)
            {
                Environment.SetEnvironmentVariable("DOTNET_CLI_HOME", "/tmp/dotnet");
                Environment.SetEnvironmentVariable("HOME", "/tmp");
                Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1");
            }

            var builder = WebApplication.CreateBuilder(args);

            // Hosters (Upsun, Render, Docker) tell us the port via PORT.
            // Locally PORT is not set, so launchSettings.json decides.
            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrWhiteSpace(port))
            {
                builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            }

            // ------------------------------------------------------------------
            // Database: ONE pool-tuned connection string for everything.
            // ------------------------------------------------------------------
            var rawConnectionString = builder.Configuration.GetConnectionString(ConnectionName);
            if (string.IsNullOrWhiteSpace(rawConnectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{ConnectionName}' is missing. Set it with user-secrets " +
                    $"(dotnet user-secrets set \"ConnectionStrings:{ConnectionName}\" \"...\") or with the " +
                    $"environment variable ConnectionStrings__{ConnectionName}.");
            }

            var dbSection = builder.Configuration.GetSection("Database");
            var maxPool = dbSection.GetValue<uint>("MaxPoolSize", 10);
            var minPool = Math.Min(dbSection.GetValue<uint>("MinPoolSize", 1), maxPool);
            var keepAliveSeconds = dbSection.GetValue<int>("KeepAliveSeconds", 30);

            var connectionStringBuilder = new MySqlConnectionStringBuilder(rawConnectionString)
            {
                Pooling = true,                         // connections are reused between requests
                MinimumPoolSize = minPool,              // at least one connection stays open
                MaximumPoolSize = maxPool,              // stay below the hoster's connection limit
                ConnectionReset = true,                 // clean session state when a connection is reused
                ConnectionIdleTimeout = dbSection.GetValue<uint>("ConnectionIdleTimeoutSeconds", 60),
                ConnectionLifeTime = dbSection.GetValue<uint>("ConnectionLifetimeSeconds", 300),
                Keepalive = dbSection.GetValue<uint>("TcpKeepAliveSeconds", 30),
                ConnectionTimeout = dbSection.GetValue<uint>("ConnectionTimeoutSeconds", 30),
                DefaultCommandTimeout = dbSection.GetValue<uint>("CommandTimeoutSeconds", 60)
            };
            var connectionString = connectionStringBuilder.ConnectionString;

            // BLL.ConnectionStringProvider (used e.g. by CoinPriceUpdater) reads from IConfiguration,
            // so it must see the tuned string as well.
            builder.Configuration[$"ConnectionStrings:{ConnectionName}"] = connectionString;
            BLL.Configuration.ConnectionStringProvider.Initialize(builder.Configuration);

            builder.Services.AddSingleton(new DbSettings(
                connectionString,
                TimeSpan.FromSeconds(Math.Max(keepAliveSeconds, 5))));

            // Services only hold the connection string; every repository call opens a pooled
            // connection and gives it back. One shared instance is enough (no "new" per request).
            builder.Services.AddSingleton(new CoinService(connectionString));
            builder.Services.AddSingleton(new UserService(connectionString));
            builder.Services.AddSingleton(new WalletService(connectionString));
            builder.Services.AddSingleton(new UserCoinService(connectionString));
            builder.Services.AddSingleton(new CoinPriceHistoryService(connectionString));

            // A crashing background service must never take the whole API down (502).
            builder.Services.Configure<HostOptions>(o =>
                o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

            if (keepAliveSeconds > 0)
            {
                builder.Services.AddHostedService<DatabaseKeepAliveService>();
            }

            if (builder.Configuration.GetValue("CoinPriceUpdater:Enabled", true))
            {
                builder.Services.AddHostedService(sp =>
                    new CoinPriceUpdater(
                        sp.GetRequiredService<ILogger<CoinPriceUpdater>>(),
                        ConnectionName));
            }

            // ------------------------------------------------------------------
            // Logging directory (the application directory may be read-only)
            // ------------------------------------------------------------------
            var logDirectory = builder.Configuration["Log:Directory"]
                ?? Path.Combine(Path.GetTempPath(), "SEP_WebApplication", "Data");

            Directory.CreateDirectory(logDirectory);
            BLL.Logger.Log.Path = Path.Combine(logDirectory, "log.txt");

            // ------------------------------------------------------------------
            // MVC / JSON
            // ------------------------------------------------------------------
            builder.Services
                .AddControllers()
                .AddJsonOptions(o =>
                {
                    // Never send password hashes / salts to a client, whichever endpoint returns a User.
                    var resolver = o.JsonSerializerOptions.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
                    o.JsonSerializerOptions.TypeInfoResolver = resolver.WithAddedModifier(HideSecrets);
                });

            // CORS: set "Cors:Origins" (array) in production to restrict it, otherwise any origin is allowed.
            var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();
            builder.Services.AddCors(o => o.AddPolicy("Client", p =>
            {
                if (origins is { Length: > 0 })
                    p.WithOrigins(origins);
                else
                    p.AllowAnyOrigin();

                p.AllowAnyHeader().AllowAnyMethod();
            }));

            var app = builder.Build();

            // Anything a controller does not catch ends up here: logged + clean JSON 500.
            app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
            {
                var error = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (error is not null)
                {
                    await Log.WriteLine(
                        $"UNHANDLED {ctx.Request.Method} {ctx.Request.Path}: {error}", "ERROR");
                }

                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await ctx.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
            }));

            app.UseCors("Client");

            app.MapControllers();

            app.MapGet("/health", () =>
                Results.Ok(new
                {
                    status = "OK",
                    application = "SEP_WebApplication"
                }));

            app.MapGet("/health/db", async (DbSettings db, CancellationToken ct) =>
            {
                try
                {
                    await DatabaseKeepAliveService.PingAsync(db.ConnectionString, ct);
                    return Results.Ok(new { status = "OK" });
                }
                catch (Exception ex)
                {
                    await Log.WriteLine($"Health check (db) failed: {ex}", "ERROR");
                    return Results.Json(new { status = "UNAVAILABLE" },
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }
            });

            app.Run();
        }

        private static void HideSecrets(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Type != typeof(User))
                return;

            foreach (var property in typeInfo.Properties)
            {
                if (string.Equals(property.Name, "PassHash", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(property.Name, "PassSalt", StringComparison.OrdinalIgnoreCase))
                {
                    property.ShouldSerialize = static (_, _) => false;
                }
            }
        }
    }
}
