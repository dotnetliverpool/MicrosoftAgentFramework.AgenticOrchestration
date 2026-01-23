using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;
using MicrosoftAgentFramework.Services.OpenMeteo;

namespace MicrosoftAgentFramework.Agent.Composer;

public class AgentAsToolComposer(IAgentProvider agentProvider, IServiceProvider serviceProvider) : IAgentComposer
{
    public AgentName Name => AgentName.AgentAsTool;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = "You are a country geodata and weather expert.";

        var countriesNowApiClient = serviceProvider.GetRequiredService<LoggingCountriesNowApiClient>();
        var dateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();
        var weatherApiClient = serviceProvider.GetRequiredService<LoggingOpenMeteoClient>();

        // Create Currency Agent
        var currencyAgent = CreateCurrencyAgent(countriesNowApiClient);
        
        // Create Population Agent
        var populationAgent = CreatePopulationAgent(countriesNowApiClient);
        
        // Create Weather Agent
        var weatherAgent = CreateWeatherAgent(weatherApiClient);

        // Convert agents to tools for the orchestrator agent
        List<AITool> agentTools = new List<AITool>()
        {
            currencyAgent.AsAIFunction(new AIFunctionFactoryOptions
            {
                Name = "currency_agent",
                Description = "An agent specialized in currency information for countries. Use this to get currency data for any country."
            }),
            
            populationAgent.AsAIFunction(new AIFunctionFactoryOptions
            {
                Name = "population_agent",
                Description = "An agent specialized in population data for countries and cities. Use this to get population statistics and data."
            }),
            
            weatherAgent.AsAIFunction(new AIFunctionFactoryOptions
            {
                Name = "weather_agent",
                Description = "An agent specialized in weather data. Use this to get current weather and forecasts for locations."
            }),
            
            AIFunctionFactory.Create(
                () => Task.FromResult(dateTimeProvider.UtcNow),
                name: "get_current_utc_time",
                description: "Gets the current UTC date and time")
        };

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: agentTools,
            contextProviderFactory: null);
    }

    private ChatClientAgent CreateCurrencyAgent(LoggingCountriesNowApiClient countriesNowApiClient)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = "CurrencyAgent"
        };

        const string instructions = "You are an agent that specializes in currency information for countries.";

        List<AITool> tools = new List<AITool>()
        {
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryCurrencyAsync,
                name: "get_country_currency",
                description: "Gets a single country and its currency")
        };

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: tools,
            contextProviderFactory: null);
    }

    private ChatClientAgent CreatePopulationAgent(LoggingCountriesNowApiClient countriesNowApiClient)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = "PopulationAgent"
        };

        const string instructions = "You are an agent that specializes in population data for countries and cities.";

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
        };

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: tools,
            contextProviderFactory: null);
    }

    private ChatClientAgent CreateWeatherAgent(LoggingOpenMeteoClient weatherApiClient)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = "WeatherAgent"
        };

        const string instructions = "You are an agent that specializes in weather data for locations around the world.";

        List<AITool> tools = new List<AITool>()
        {
            AIFunctionFactory.Create(
                (string location) =>
                    weatherApiClient.QueryAsync(location),
                name: "get_weather_by_location",
                description: "Gets weather forecast for a location by name (e.g., city name or coordinates as string)"),
            
            AIFunctionFactory.Create(
                (float latitude, float longitude) =>
                    weatherApiClient.QueryAsync(latitude, longitude),
                name: "get_weather_by_coordinates",
                description: "Gets weather forecast for a location by latitude and longitude coordinates")
        };

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: tools,
            contextProviderFactory: null);
    }
}
