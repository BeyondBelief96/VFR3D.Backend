using System.Xml.Linq;

namespace VFR3D.Infrastructure.Configuration
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
            // Check if Host is a PostgreSQL URL
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
                catch (Exception)
                {
                    // If URL parsing fails, fall back to standard format
                }
            }

            // Standard format
            return $"Host={Host};" +
                   $"Database={DatabaseName};" +
                   $"Username={Username};" +
                   $"Password={Password};" +
                   $"Port={Port};" +
                   "SSL Mode=Require;" +
                   "Trust Server Certificate=true";
        }
    }
}
