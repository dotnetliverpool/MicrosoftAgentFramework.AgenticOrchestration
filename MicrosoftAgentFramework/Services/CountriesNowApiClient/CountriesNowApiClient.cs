using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MicrosoftAgentFramework.Services.CountriesNowApiClient.Models;

namespace MicrosoftAgentFramework.Services.CountriesNowApiClient;

public class CountriesNowApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://countriesnow.space/api/v0.1/";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly JsonSerializerOptions SerializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CountriesNowApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    // Population Endpoints

    /// <summary>
    /// Get a single city and its population data
    /// </summary>
    public async Task<ApiResponse<CityPopulationData>> GetCityPopulationAsync(
        CityPopulationRequest request, 
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SerializeOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            "countries/population/cities", 
            content, 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<CityPopulationData>>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    /// Filter cities and population data by country, order, and limit
    /// </summary>
    public async Task<ApiResponse<List<CityPopulationData>>> FilterCitiesAsync(
        FilterCitiesRequest request, 
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SerializeOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            "countries/population/cities/filter", 
            content, 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<CityPopulationData>>>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    /// Get all cities and their population data
    /// </summary>
    public async Task<ApiResponse<List<CityPopulationData>>> GetAllCitiesPopulationAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            "countries/population/cities", 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<CityPopulationData>>>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    /// Filter countries and population data by year, limit, gt, lt, order, and orderBy
    /// </summary>
    public async Task<ApiResponse<List<FilteredCountryPopulationData>>> FilterPopulationAsync(
        FilterPopulationRequest request, 
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SerializeOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            "countries/population/filter", 
            content, 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<FilteredCountryPopulationData>>>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    /// Get a single country and its population data
    /// </summary>
    public async Task<ApiResponse<CountryPopulationData>> GetCountryPopulationAsync(
        CountryPopulationRequest request, 
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SerializeOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            "countries/population", 
            content, 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<CountryPopulationData>>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    /// Get all countries and their respective population data (1961-2018)
    /// </summary>
    public async Task<ApiResponse<List<CountryPopulationData>>> GetAllCountriesPopulationAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            "countries/population", 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<CountryPopulationData>>>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    // Currency Endpoints

    /// <summary>
    /// Get all countries and their currencies
    /// </summary>
    public async Task<ApiResponse<List<CountryCurrencyData>>> GetAllCountriesCurrencyAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            "countries/currency", 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<CountryCurrencyData>>>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    /// Get a single country and its currency
    /// </summary>
    public async Task<ApiResponse<CountryCurrencyData>> GetCountryCurrencyAsync(
        CountryCurrencyRequest request, 
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SerializeOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            "countries/currency", 
            content, 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<CountryCurrencyData>>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    // Position Endpoints

    /// <summary>
    /// Get a single country and its positions (latitude and longitude)
    /// </summary>
    public async Task<ApiResponse<CountryPositionData>> GetCountryPositionAsync(
        CountryPositionRequest request, 
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, SerializeOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(
            "countries/positions", 
            content, 
            cancellationToken);
        
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<CountryPositionData>>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}
