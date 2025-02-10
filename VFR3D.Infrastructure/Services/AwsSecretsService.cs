using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Options;
using System.Text.Json;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services
{
    public class AwsSecretsService : IAwsSecretsService
    {
        private readonly IAmazonSecretsManager _secretsManager;
        private readonly string _sslCertSecretName;

        public AwsSecretsService(IOptions<AwsSettings> awsSettings)
        {
            var config = new AmazonSecretsManagerConfig
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(awsSettings.Value.Region),
            };

            _secretsManager = new AmazonSecretsManagerClient(awsSettings.Value.AccessKeyId, awsSettings.Value.SecretAccessKey, config);
            _sslCertSecretName = awsSettings.Value.DbSslCertSecretName;
        }

        public async Task<string> GetDatabaseSslCertificateAsync()
        {
            try
            {
                var request = new GetSecretValueRequest
                {
                    SecretId = _sslCertSecretName,
                };

                var response = await _secretsManager.GetSecretValueAsync(request);

                if(string.IsNullOrEmpty(response.SecretString))
                {
                    throw new Exception("Secret string is empty.");
                }

                var secretData = JsonSerializer.Deserialize<JsonElement>(response.SecretString);
                return secretData.GetProperty("VFR3D_DB_SSL_CERT").GetString() ?? throw new Exception("SSL Certificate not found in secret.");
            }
            catch(Exception ex)
            {
                throw new Exception("Error retrieving SSL certificate", ex);
            }
        }
    }
}
