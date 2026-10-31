using Microsoft.Extensions.Configuration;

namespace BLL.Configuration
{
    public static class ConnectionStringProvider
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public static string GetConnectionString(string name = "LocaleConnection")
        {
            if (_configuration == null)
                throw new InvalidOperationException("ConnectionStringProvider has not been initialized. Call Initialize() in Program.cs.");
            var connectionString = _configuration.GetConnectionString(name);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Connection string '{name}' not found in the configuration.");
            return connectionString;
        }
    }
}
