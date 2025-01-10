namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface IAwsSecretsService
    {
        Task<string> GetDatabaseSslCertificateAsync();
    }
}
