namespace VFR3D.Infrastructure.Services.CronJobServices.ArcGisServices.Models
{
    public class ArcGisResponse<T>
    {
        public List<ArcGisFeature<T>> Features { get; set; } = new();
    }
}
