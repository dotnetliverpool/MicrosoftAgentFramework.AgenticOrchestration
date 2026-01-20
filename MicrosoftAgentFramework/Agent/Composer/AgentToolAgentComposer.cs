using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;

namespace MicrosoftAgentFramework.Agent.Composer;

public class AgentToolAgentComposer(IAgentProvider agentProvider, IServiceProvider serviceProvider) : IAgentComposer
{
    public AgentName Name => AgentName.AgentWithTools;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = "You are an agent that responds with text about countries.";

        var countriesNowApiClient = serviceProvider.GetRequiredService<CountriesNowApiClient>();
        var dateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();

        List<AITool> tools = new List<AITool>()
        {
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCityPopulationAsync,
                name: "get_city_population",
                description: "Gets a single city and its population data"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.FilterCitiesAsync,
                name: "filter_cities_population",
                description: "Filters cities and population data by country, order, and limit"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetAllCitiesPopulationAsync,
                name: "get_all_cities_population",
                description: "Gets all cities and their population data"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.FilterPopulationAsync,
                name: "filter_countries_population",
                description: "Filters countries and population data by year, limit, greater than, less than, order, and orderBy"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryPopulationAsync,
                name: "get_country_population",
                description: "Gets a single country and its population data"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetAllCountriesPopulationAsync,
                name: "get_all_countries_population",
                description: "Gets all countries and their respective population data (1961-2018)"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetAllCountriesCurrencyAsync,
                name: "get_all_countries_currency",
                description: "Gets all countries and their currencies"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryCurrencyAsync,
                name: "get_country_currency",
                description: "Gets a single country and its currency"),
            
            AIFunctionFactory.Create(
                () => Task.FromResult(dateTimeProvider.UtcNow),
                name: "get_current_utc_time",
                description: "Gets the current UTC date and time")
        };

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: tools,
            contextProviderFactory: null);
    }
}
