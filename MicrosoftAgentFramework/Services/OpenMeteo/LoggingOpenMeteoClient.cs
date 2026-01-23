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
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("OpenMeteoClient.QueryAsync called with Location: {Location}", location);
        
        try
        {
            var result = await _innerClient.QueryAsync(location);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("OpenMeteoClient.QueryAsync completed in {Duration}ms for Location: {Location}", 
                duration, location);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "OpenMeteoClient.QueryAsync failed after {Duration}ms for Location: {Location}", 
                duration, location);
            throw;
        }
    }

    public async Task<WeatherForecast?> QueryAsync(float latitude, float longitude, CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("OpenMeteoClient.QueryAsync called with Coordinates: Lat={Latitude}, Long={Longitude}", 
            latitude, longitude);
        
        try
        {
            var result = await _innerClient.QueryAsync(latitude, longitude);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("OpenMeteoClient.QueryAsync completed in {Duration}ms for Coordinates: Lat={Latitude}, Long={Longitude}", 
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
