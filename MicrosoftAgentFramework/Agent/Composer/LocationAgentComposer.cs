using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;

namespace MicrosoftAgentFramework.Agent.Composer;

public class LocationAgentComposer(IAgentProvider agentProvider, IServiceProvider serviceProvider) : IAgentComposer
{
    public AgentName Name => AgentName.LocationAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are a location and country data expert.
            You answer questions about countries, cities, populations, currencies, and geographical data.
            Provide detailed, helpful responses about any country-related queries.
            """;

        var countriesNowApiClient = serviceProvider.GetRequiredService<CountriesNowApiClient>();
        var dateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();

        List<AITool> tools = new List<AITool>()
        {
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCityPopulationAsync,
                name: "get_city_population",
                description: "Gets a single city and its population data"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryPopulationAsync,
                name: "get_country_population",
                description: "Gets a single country and its population data"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryCurrencyAsync,
                name: "get_country_currency",
                description: "Gets a single country and its currency"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryPositionAsync,
                name: "get_country_position",
                description: "Gets a country's latitude and longitude coordinates"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetAllCountriesCurrencyAsync,
                name: "get_all_countries_currency",
                description: "Gets all countries and their currencies"),
            
            AIFunctionFactory.Create(
                () => Task.FromResult(dateTimeProvider.UtcNow),
                name: "get_current_utc_time",
                description: "Gets the current UTC date and time")
        };

        return agentProvider.GetAgent(aiModel, instructions, tools);
    }
}
