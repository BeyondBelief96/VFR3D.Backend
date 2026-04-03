namespace VFR3D.Infrastructure.Settings
{
    public class DatabaseSettings
    {
        /// <summary>
        /// Full connection string. If provided, this takes precedence over individual properties.
        /// </summary>
        public string? ConnectionString { get; set; }

        public string Host { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; } = 5432;
        public string SslCertificate { get; set; } = string.Empty;

        public string GetConnectionString()
        {
            // If a full connection string is provided, use it directly
            if (!string.IsNullOrWhiteSpace(ConnectionString))
            {
                return ConnectionString;
            }

            if (Host.StartsWith("postgresql://"))
            {
                    var uri = new Uri(Host);
                    var userInfo = uri.UserInfo.Split(':');
                    var database = uri.AbsolutePath.TrimStart('/');
                    var host = uri.Host;
                    return $"Host={host};" +
                           $"Database={database};" +
                           $"Username={userInfo[0]};" +
                           $"Password={userInfo[1]};" +
                           $"Port={Port};" +
                           "SSL Mode=Require;" +
                           "Trust Server Certificate=true";
            }

            // Local development
            var connectionString = $"Host={Host};" +
                                 $"Database={Database};" +
                                 $"Username={Username};" +
                                 $"Password={Password};" +
                                 $"Port={Port}";

            // Only add SSL settings for non-local connections
            if (!Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) &&
                !Host.Equals("127.0.0.1") &&
                !Host.Equals("db"))
            {
                connectionString += ";SSL Mode=Require;Trust Server Certificate=true";
            }

            return connectionString;
        }
    }
}