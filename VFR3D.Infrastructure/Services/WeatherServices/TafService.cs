using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.WeatherServices;

public class TafService : ITafService
{
    private readonly VFR3DDbContext _dbContext;
    private readonly ILogger<TafService> _logger;

    public TafService(VFR3DDbContext dbContext, ILogger<TafService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<TafDto> GetTafByIcaoCode(string icaoCodeOrIdent)
    {
        var taf = await _dbContext.Tafs.FirstOrDefaultAsync(t => t.StationId == icaoCodeOrIdent.ToUpper());

        if (taf == null)
        {
            var airport = await _dbContext.Airports
                .FirstOrDefaultAsync(a => a.ArptId == icaoCodeOrIdent.ToUpper() ||
                                          a.IcaoId == icaoCodeOrIdent.ToUpper());

            if (airport == null)
            {
                throw new AirportNotFoundException(icaoCodeOrIdent);
            }

            var modifiedIdent = airport.StateCode switch
            {
                "AK" or "HI" => $"P{airport.ArptId}",
                _ => $"K{airport.ArptId}"
            };

            taf = await _dbContext.Tafs
                .FirstOrDefaultAsync(t => t.StationId == modifiedIdent);
        }
        
        if (taf == null)
        {
            throw new TafNotFoundException(icaoCodeOrIdent);
        }

        return TafMapper.ToDto(taf);
    }
}