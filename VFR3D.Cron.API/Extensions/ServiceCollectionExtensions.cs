using Amazon.Runtime;
using Amazon.S3;
using VFR3D.Infrastructure.Configuration;

namespace VFR3D.Cron.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAwsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AwsSettings>(configuration.GetSection("AWS"));

            var awsOptions = configuration.GetAWSOptions();
            var accessKey = configuration["AWS:AccessKeyId"];
            var secretKey = configuration["AWS:SecretAccessKey"];

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            {
                awsOptions.Credentials = new BasicAWSCredentials(accessKey, secretKey);
            }

            services.AddDefaultAWSOptions(awsOptions);
            services.AddAWSService<IAmazonS3>();

            return services;
        }
    }
}
