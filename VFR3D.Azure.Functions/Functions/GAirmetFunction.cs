using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions.Functions;

public class GAirmetFunction
{
    private readonly ILogger _logger;
    private readonly IAviationWeatherService<GAirmet> _gairmetService;

    public GAirmetFunction(IAviationWeatherService<GAirmet> gairmetService, ILoggerFactory loggerFactory)
    {
        _gairmetService = gairmetService ?? throw new ArgumentNullException(nameof(gairmetService));
        _logger = loggerFactory.CreateLogger<GAirmetFunction>();
    }

    [Function("GAirmetFunction")]
    public async Task Run([TimerTrigger("0 */30 * * * *", RunOnStartup = true)] TimerInfo myTimer, FunctionContext context)
    {
        _logger.LogInformation("G-AIRMET Function executed at: {time}", DateTime.UtcNow);
        try
        {
            await _gairmetService.PollWeatherDataAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing G-AIRMET service");
            throw;
        }
    }
}
