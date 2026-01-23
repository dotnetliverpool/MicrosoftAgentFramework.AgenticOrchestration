using Microsoft.Extensions.Logging;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient.Models;

namespace MicrosoftAgentFramework.Services.CountriesNowApiClient;

public class LoggingCountriesNowApiClient
{
    private readonly CountriesNowApiClient _innerClient;
    private readonly ILogger<LoggingCountriesNowApiClient> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoggingCountriesNowApiClient(
        CountriesNowApiClient innerClient,
        ILogger<LoggingCountriesNowApiClient> logger,
        IDateTimeProvider dateTimeProvider)
    {
        _innerClient = innerClient;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ApiResponse<CityPopulationData>> GetCityPopulationAsync(
        CityPopulationRequest request, 
        CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("CountriesNowApiClient.GetCityPopulationAsync called with City: {City}", request.City);
        
        try
        {
            var result = await _innerClient.GetCityPopulationAsync(request, cancellationToken);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("CountriesNowApiClient.GetCityPopulationAsync completed in {Duration}ms", duration);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "CountriesNowApiClient.GetCityPopulationAsync failed after {Duration}ms", duration);
            throw;
        }
    }

    public async Task<ApiResponse<List<CityPopulationData>>> FilterCitiesAsync(
        FilterCitiesRequest request, 
        CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("CountriesNowApiClient.FilterCitiesAsync called with Country: {Country}, Limit: {Limit}", 
            request.Country, request.Limit);
        
        try
        {
            var result = await _innerClient.FilterCitiesAsync(request, cancellationToken);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("CountriesNowApiClient.FilterCitiesAsync completed in {Duration}ms", duration);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "CountriesNowApiClient.FilterCitiesAsync failed after {Duration}ms", duration);
            throw;
        }
    }

    public async Task<ApiResponse<List<CityPopulationData>>> GetAllCitiesPopulationAsync(
        CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("CountriesNowApiClient.GetAllCitiesPopulationAsync called");
        
        try
        {
            var result = await _innerClient.GetAllCitiesPopulationAsync(cancellationToken);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("CountriesNowApiClient.GetAllCitiesPopulationAsync completed in {Duration}ms", duration);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "CountriesNowApiClient.GetAllCitiesPopulationAsync failed after {Duration}ms", duration);
            throw;
        }
    }

    public async Task<ApiResponse<List<FilteredCountryPopulationData>>> FilterPopulationAsync(
        FilterPopulationRequest request, 
        CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("CountriesNowApiClient.FilterPopulationAsync called with Year: {Year}, Limit: {Limit}", 
            request.Year, request.Limit);
        
        try
        {
            var result = await _innerClient.FilterPopulationAsync(request, cancellationToken);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("CountriesNowApiClient.FilterPopulationAsync completed in {Duration}ms", duration);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "CountriesNowApiClient.FilterPopulationAsync failed after {Duration}ms", duration);
            throw;
        }
    }

    public async Task<ApiResponse<CountryPopulationData>> GetCountryPopulationAsync(
        CountryPopulationRequest request, 
        CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("CountriesNowApiClient.GetCountryPopulationAsync called with Country: {Country}", request.Country);
        
        try
        {
            var result = await _innerClient.GetCountryPopulationAsync(request, cancellationToken);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("CountriesNowApiClient.GetCountryPopulationAsync completed in {Duration}ms", duration);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "CountriesNowApiClient.GetCountryPopulationAsync failed after {Duration}ms", duration);
            throw;
        }
    }

    public async Task<ApiResponse<List<CountryPopulationData>>> GetAllCountriesPopulationAsync(
        CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("CountriesNowApiClient.GetAllCountriesPopulationAsync called");
        
        try
        {
            var result = await _innerClient.GetAllCountriesPopulationAsync(cancellationToken);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("CountriesNowApiClient.GetAllCountriesPopulationAsync completed in {Duration}ms", duration);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "CountriesNowApiClient.GetAllCountriesPopulationAsync failed after {Duration}ms", duration);
            throw;
        }
    }

    public async Task<ApiResponse<List<CountryCurrencyData>>> GetAllCountriesCurrencyAsync(
        CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("CountriesNowApiClient.GetAllCountriesCurrencyAsync called");
        
        try
        {
            var result = await _innerClient.GetAllCountriesCurrencyAsync(cancellationToken);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("CountriesNowApiClient.GetAllCountriesCurrencyAsync completed in {Duration}ms", duration);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "CountriesNowApiClient.GetAllCountriesCurrencyAsync failed after {Duration}ms", duration);
            throw;
        }
    }

    public async Task<ApiResponse<CountryCurrencyData>> GetCountryCurrencyAsync(
        CountryCurrencyRequest request, 
        CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("CountriesNowApiClient.GetCountryCurrencyAsync called with Country: {Country}", request.Country);
        
        try
        {
            var result = await _innerClient.GetCountryCurrencyAsync(request, cancellationToken);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("CountriesNowApiClient.GetCountryCurrencyAsync completed in {Duration}ms", duration);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "CountriesNowApiClient.GetCountryCurrencyAsync failed after {Duration}ms", duration);
            throw;
        }
    }

    public async Task<ApiResponse<CountryPositionData>> GetCountryPositionAsync(
        CountryPositionRequest request, 
        CancellationToken cancellationToken = default)
    {
        var startTime = _dateTimeProvider.UtcNow;
        _logger.LogInformation("CountriesNowApiClient.GetCountryPositionAsync called with Country: {Country}", request.Country);
        
        try
        {
            var result = await _innerClient.GetCountryPositionAsync(request, cancellationToken);
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("CountriesNowApiClient.GetCountryPositionAsync completed in {Duration}ms", duration);
            return result;
        }
        catch (Exception ex)
        {
            var duration = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "CountriesNowApiClient.GetCountryPositionAsync failed after {Duration}ms", duration);
            throw;
        }
    }
}
