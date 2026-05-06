using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;

namespace MicrosoftAgentFramework.Agent.Composer;

public class CountryExtractorComposer(IAgentProvider agentProvider) : IAgentComposer
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
            
            Extract country names and ISO codes from user messages.
            If no hint that points to an existing country is found in the message, set Success=false and populate UserPromptMessage with a concise, friendly question asking for the country name.
            
            Use standard 3-letter ISO country codes (e.g., "USA", "GBR", "FRA").
            """;
        

        return agentProvider.GetAgent(aiModel, instructions);
    }
}
