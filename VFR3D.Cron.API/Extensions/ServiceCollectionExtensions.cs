using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Cron.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAwsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AwsSettings>(configuration.GetSection("AWS"));
            var awsSettings = configuration.GetSection("AWS").Get<AwsSettings>();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var regionEndpoint = RegionEndpoint.GetBySystemName(awsSettings?.Region ?? "us-east-1");

            var s3Config = new AmazonS3Config
            {
                RegionEndpoint = regionEndpoint,
                ForcePathStyle = true
            };

            // Configure for local development with LocalStack
            if (environment?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true)
            {
                s3Config.ServiceURL = "http://localstack:4566";
                s3Config.ForcePathStyle = true;
                s3Config.UseHttp = true;
                s3Config.DisableHostPrefixInjection = true;
                s3Config.UseArnRegion = false;

                var localCredentials = new BasicAWSCredentials(
                    awsSettings?.AccessKeyId ?? "test123",
                    awsSettings?.SecretAccessKey ?? "test123"
                );
                services.AddSingleton<IAmazonS3>(new AmazonS3Client(localCredentials, s3Config));
            }
            else
            {
                if (string.IsNullOrEmpty(awsSettings?.AccessKeyId) || string.IsNullOrEmpty(awsSettings?.SecretAccessKey))
                {
                    services.AddSingleton<IAmazonS3>(new AmazonS3Client(s3Config));
                }
                else
                {
                    var credentials = new BasicAWSCredentials(
                        awsSettings.AccessKeyId,
                        awsSettings.SecretAccessKey
                    );
                    services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, s3Config));
                }
            }

            return services;
        }
    }
}