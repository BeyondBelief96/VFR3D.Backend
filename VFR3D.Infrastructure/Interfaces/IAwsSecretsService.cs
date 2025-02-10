namespace VFR3D.Infrastructure.Interfaces
{
    public interface IAwsSecretsService
    {
        Task<string> GetDatabaseSslCertificateAsync();
    }
}
