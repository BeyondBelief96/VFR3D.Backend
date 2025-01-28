namespace VFR3D.Infrastructure.Settings
{
    public class AwsSettings
    {
        public string Region { get; set; } = string.Empty;

        public string AccessKeyId { get; set; } = string.Empty;

        public string SecretAccessKey { get; set; } = string.Empty;

        public string? ServiceUrl { get; set; } = string.Empty;

        public string ChartSupplementsBucketName { get; set; } = string.Empty;

        public string AirportDiagramsBucketName { get; set; } = string.Empty;

        public string DbSslCertSecretName { get; set; } = string.Empty;
    }
}
