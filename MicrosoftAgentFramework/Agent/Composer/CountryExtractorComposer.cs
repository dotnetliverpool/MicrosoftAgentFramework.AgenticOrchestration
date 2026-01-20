using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;

namespace MicrosoftAgentFramework.Agent.Composer;

public class CountryExtractorComposer(IAgentProvider agentProvider, IServiceProvider serviceProvider) : IAgentComposer
{
    public AgentName Name => AgentName.CountryExtractor;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are an agent specialized in extracting country information from user messages.
            
            Extract country names and IATA codes from user messages.
            If the user message does not contain country information, set Success=false and provide a friendly prompt in UserPromptMessage asking for the country name.
            If the user asks about anything other than country information, refuse politely and only return a prompt asking for the country name (Success=false).
            
            Use standard 3-letter IATA country codes (e.g., "USA", "GBR", "FRA").
            """;

        var countriesNowApiClient = serviceProvider.GetRequiredService<CountriesNowApiClient>();
        var dateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();

        List<AITool> tools = new List<AITool>()
        {
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryPositionAsync,
                name: "get_country_position",
                description: "Gets a country's latitude and longitude coordinates"),
            
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
