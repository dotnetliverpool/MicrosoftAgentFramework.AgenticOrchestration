using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;
using MicrosoftAgentFramework.Services.CountriesNowApiClient.Models;

namespace MicrosoftAgentFramework.Agent.Tools;

public class CountryTools(LoggingCountriesNowApiClient client)
{
    public AITool CityPopulation { get; } = AIFunctionFactory.Create(
        (string city, CancellationToken cancellationToken) =>
            client.GetCityPopulationAsync(new CityPopulationRequest { City = city }, cancellationToken),
        name: "get_city_population",
        description: "Gets historical population data for a city.");

    public AITool CountryPopulation { get; } = AIFunctionFactory.Create(
        (string country, CancellationToken cancellationToken) =>
            client.GetCountryPopulationAsync(new CountryPopulationRequest { Country = country }, cancellationToken),
        name: "get_country_population",
        description: "Gets historical population data for a country.");

    public AITool CountryCurrency { get; } = AIFunctionFactory.Create(
        (string country, CancellationToken cancellationToken) =>
            client.GetCountryCurrencyAsync(new CountryCurrencyRequest { Country = country }, cancellationToken),
        name: "get_country_currency",
        description: "Gets currency information for a country.");

    public AITool CountryPosition { get; } = AIFunctionFactory.Create(
        (string country, CancellationToken cancellationToken) =>
            client.GetCountryPositionAsync(new CountryPositionRequest { Country = country }, cancellationToken),
        name: "get_country_position",
        description: "Gets a country's latitude and longitude coordinates.");

    public AITool FilterCitiesPopulation { get; } = AIFunctionFactory.Create(
        (string country, int limit, CancellationToken cancellationToken) =>
            client.FilterCitiesAsync(
                new FilterCitiesRequest
                {
                    Country = country,
                    Limit = limit,
                    Order = "desc",
                    OrderBy = "name"
                },
                cancellationToken),
        name: "filter_cities_population",
        description: "Gets top cities and population data for a country, limited by count.");
}
