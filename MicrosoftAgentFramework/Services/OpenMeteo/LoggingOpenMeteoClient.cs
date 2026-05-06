using Microsoft.Extensions.Logging;
using OpenMeteo;
using MicrosoftAgentFramework.Services;

namespace MicrosoftAgentFramework.Services.OpenMeteo;

public class LoggingOpenMeteoClient
{
    private readonly OpenMeteoClient _innerClient;
    private readonly ILogger<LoggingOpenMeteoClient> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoggingOpenMeteoClient(
        OpenMeteoClient innerClient,
        ILogger<LoggingOpenMeteoClient> logger,
        IDateTimeProvider dateTimeProvider)
    {
        _innerClient = innerClient;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<WeatherForecast?> QueryAsync(string location, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _innerClient.QueryAsync(location);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenMeteoClient.QueryAsync failed for Location: {Location}", location);
            throw;
        }
    }

    public async Task<WeatherForecast?> QueryAsync(float latitude, float longitude, CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogDebug("OpenMeteoClient.QueryAsync called with Coordinates: Lat={Latitude}, Long={Longitude}", 
            latitude, longitude);
        
        try
        {
            var result = await _innerClient.QueryAsync(latitude, longitude);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogDebug("OpenMeteoClient.QueryAsync completed in {Duration}ms for Coordinates: Lat={Latitude}, Long={Longitude}", 
                duration, latitude, longitude);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "OpenMeteoClient.QueryAsync failed after {Duration}ms for Coordinates: Lat={Latitude}, Long={Longitude}", 
                duration, latitude, longitude);
            throw;
        }
    }
}
