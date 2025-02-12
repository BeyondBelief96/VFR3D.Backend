namespace VFR3D.Infrastructure.Interfaces
{
    public interface IAirspaceCronService<TEntity>
    {
        Task UpdateAirspacesAsync(CancellationToken cancellationToken = default);
    }
}
