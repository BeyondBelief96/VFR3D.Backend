namespace VFR3D.Infrastructure.Dtos
{
    public record StateInfoDto
    {
        public string StateCode { get; init; } = string.Empty;
        public string StateName { get; init; } = string.Empty;
    }
}
