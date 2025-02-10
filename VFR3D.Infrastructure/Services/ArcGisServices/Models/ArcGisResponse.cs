namespace VFR3D.Infrastructure.Services.ArcgisServices.Models
{
    public class ArcGisResponse<T>
    {
        public List<ArcGisFeature<T>> Features { get; set; } = new();
    }
}
