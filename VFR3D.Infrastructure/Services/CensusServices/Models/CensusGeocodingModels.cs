namespace VFR3D.Infrastructure.Services.CensusServices.Models
{
    public class CensusResponse
    {
        public CensusResult? Result { get; set; }
    }

    public class CensusResult
    {
        public List<GeographicInfo>? GeographicData { get; set; }
    }

    public class GeographicInfo
    {
        public StateInfo State { get; set; } = new();
    }

    public class StateInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Fips { get; set; } = string.Empty;
    }
}
