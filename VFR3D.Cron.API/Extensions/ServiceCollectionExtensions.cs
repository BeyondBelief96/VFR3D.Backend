using Amazon;
using Amazon.Extensions.NETCore.Setup;
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

            var s3Config = new AmazonS3Config
            {
                ServiceURL = awsSettings?.ServiceUrl ?? "http://localstack:4566",
                ForcePathStyle = true,
                UseHttp = true,
                AuthenticationRegion = awsSettings?.Region ?? "us-east-1", 
                DisableHostPrefixInjection = true,
                UseArnRegion = false
            };

            var credentials = new BasicAWSCredentials(
                awsSettings?.AccessKeyId ?? "test123",
                awsSettings?.SecretAccessKey ?? "test123"
            );

            services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, s3Config));

            return services;
        }
    }
}
