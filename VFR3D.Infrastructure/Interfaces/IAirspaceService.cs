namespace VFR3D.Infrastructure.Interfaces
{
    public interface IAirspaceService<TEntity>
    {
        Task UpdateAirspacesAsync(CancellationToken cancellationToken = default);
    }
}
