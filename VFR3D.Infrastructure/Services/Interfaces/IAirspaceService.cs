namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface IAirspaceService<TEntity>
    {
        Task UpdateAirspacesAsync(CancellationToken cancellationToken = default);
    }
}
