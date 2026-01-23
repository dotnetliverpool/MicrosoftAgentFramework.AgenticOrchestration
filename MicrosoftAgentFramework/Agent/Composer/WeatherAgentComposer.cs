using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.OpenMeteo;

namespace MicrosoftAgentFramework.Agent.Composer;

public class WeatherAgentComposer(IAgentProvider agentProvider, IServiceProvider serviceProvider) : IAgentComposer
{
    public AgentName Name => AgentName.WeatherAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are a weather expert.
            You answer questions about current weather conditions, forecasts, and weather-related data.
            Provide detailed, helpful responses about weather for any location.
            """;

        var weatherApiClient = serviceProvider.GetRequiredService<LoggingOpenMeteoClient>();

        List<AITool> tools = new List<AITool>()
        {
            AIFunctionFactory.Create(
                (string location) =>
                    weatherApiClient.QueryAsync(location),
                name: "get_weather_by_location",
                description: "Gets weather forecast for a location by name"),
            
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
