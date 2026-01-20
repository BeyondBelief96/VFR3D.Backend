namespace VFR3D.Infrastructure.Services.CronJobServices.ArcGisServices.Models
{
    public class ArcGisResponse<T>
    {
        public List<ArcGisFeature<T>> Features { get; set; } = new();

        /// <summary>
        /// Indicates if there are more records available beyond the current result set.
        /// Used for pagination when the server has a max record count limit.
        /// </summary>
        public bool ExceededTransferLimit { get; set; }
    }
}
