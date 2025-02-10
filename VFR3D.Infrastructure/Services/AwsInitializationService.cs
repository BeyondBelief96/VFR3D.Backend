using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services
{
    public class AwsInitializationService : IAwsInitializationService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<AwsInitializationService> _logger;
        private readonly IOptions<AwsSettings> _settings;

        public AwsInitializationService(IAmazonS3 s3Client, IOptions<AwsSettings> awsSettings, ILogger<AwsInitializationService> logger)
        {
            _s3Client = s3Client;
            _settings = awsSettings;
            _logger = logger;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await InitializeS3BucketsAsync(cancellationToken);
        }

        private async Task InitializeS3BucketsAsync(CancellationToken cancellationToken)
        {
            var requiredBuckets = new string[]
            {
                _settings.Value.ChartSupplementsBucketName,
                _settings.Value.AirportDiagramsBucketName,
            };

            foreach (var bucket in requiredBuckets)
            {
                try
                {
                    var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucket);
                    if (!bucketExists)
                    {
                        _logger.LogInformation("Creating bucket: {BucketName}", bucket);

                        var configuration = new PutBucketRequest
                        {
                            BucketName = bucket,
                            UseClientRegion = false
                        };

                        // Only add LocationConstraint for non-us-east-1 regions
                        if (_settings.Value.Region != "us-east-1")
                        {
                            var region = _settings.Value.Region.ToLowerInvariant() switch
                            {
                                "us-east-2" => S3Region.USEast2,
                                "us-west-1" => S3Region.USWest1,
                                "us-west-2" => S3Region.USWest2,
                                "eu-west-1" => S3Region.EUWest1,
                                "eu-central-1" => S3Region.EUCentral1,
                                "ap-southeast-1" => S3Region.APSoutheast1,
                                "ap-southeast-2" => S3Region.APSoutheast2,
                                "ap-northeast-1" => S3Region.APNortheast1,
                                "sa-east-1" => S3Region.SAEast1,
                                _ => null
                            };

                            if (region != null)
                            {
                                configuration.BucketRegion = region;
                            }
                        }

                        await _s3Client.PutBucketAsync(configuration, cancellationToken);
                        _logger.LogInformation("Successfully created bucket: {BucketName}", bucket);
                    }
                    else
                    {
                        _logger.LogInformation("Bucket already exists: {BucketName}", bucket);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error when creating bucket {BucketName}", bucket);
                    throw;
                }
            }
        }
    }
}
