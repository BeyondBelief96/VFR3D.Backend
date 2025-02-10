namespace VFR3D.Infrastructure.Services.ArcgisServices.Models
{
    public class ArcGisFeature<T>
    {
        public T Attributes { get; set; } = default!;
        public ArcGisGeometry? Geometry { get; set; }
    }
}
