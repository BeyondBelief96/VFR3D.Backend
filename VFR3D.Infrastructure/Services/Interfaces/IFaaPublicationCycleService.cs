using VFR3D.Domain.ValueObjects.FaaPublications;

namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface IFaaPublicationCycleService
    {
        Task<bool> ShouldRunUpdateAsync(PublicationType publicationType, DateTime currentDate);
        Task UpdateLastSuccessfulRunAsync(PublicationType publicationType, DateTime updateDate);
    }
}
