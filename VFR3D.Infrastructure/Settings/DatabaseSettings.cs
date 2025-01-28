namespace VFR3D.Infrastructure.Settings
{
    public class DatabaseSettings
    {
        public string Host { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SslCertificate { get; set; } = string.Empty;

        public string GetConnectionString()
        {
            if (Host.StartsWith("postgresql://"))
            {
                try
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
                catch
                {
                    // Fall back to standard format
                }
            }

            // Local development
            var connectionString = $"Host={Host};" +
                                 $"Database={DatabaseName};" +
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