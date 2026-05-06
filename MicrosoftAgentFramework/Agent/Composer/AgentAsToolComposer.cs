using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Agent.Tools;

namespace MicrosoftAgentFramework.Agent.Composer;

public class AgentAsToolComposer(
    IAgentProvider agentProvider,
    CountryTools countryTools,
    WeatherTools weatherTools,
    DateTimeTools dateTimeTools) : IAgentComposer
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

        const string instructions = """
            You are a country geodata and weather expert.
            Use currency_agent for questions about country currencies.
            Use population_agent for questions about city or country populations.
            Use weather_agent for weather conditions or forecasts.
            Use get_current_utc_time for time context when needed.
            Answer using only the sub-agents relevant to the question.
            """;

        // Create Currency Agent
        var currencyAgent = CreateCurrencyAgent(countryTools);
        
        // Create Population Agent
        var populationAgent = CreatePopulationAgent(countryTools);
        
        // Create Weather Agent
        var weatherAgent = CreateWeatherAgent(weatherTools);

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
            
            dateTimeTools.CurrentUtcTime
        };

        return agentProvider.GetAgent(aiModel, instructions, agentTools);
    }

    private ChatClientAgent CreateCurrencyAgent(CountryTools countryTools)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = "CurrencyAgent"
        };

        const string instructions = "You are an agent that specializes in currency information for countries.";

        List<AITool> tools = new()
        {
            countryTools.CountryCurrency
        };

        return agentProvider.GetAgent(aiModel, instructions, tools);
    }

    private ChatClientAgent CreatePopulationAgent(CountryTools countryTools)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = "PopulationAgent"
        };

        const string instructions = "You are an agent that specializes in population data for countries and cities.";

        List<AITool> tools = new()
        {
            countryTools.CityPopulation,
            countryTools.CountryPopulation
        };

        return agentProvider.GetAgent(aiModel, instructions, tools);
    }

    private ChatClientAgent CreateWeatherAgent(WeatherTools weatherTools)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = "WeatherAgent"
        };

        const string instructions = "You are an agent that specializes in weather data for locations around the world.";

        List<AITool> tools = new()
        {
            weatherTools.WeatherByLocation,
            weatherTools.WeatherByCoordinates
        };

        return agentProvider.GetAgent(aiModel, instructions, tools);
    }
}
