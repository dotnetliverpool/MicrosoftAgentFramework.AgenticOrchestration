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
            If the user message does not contain country information, set Success=false and provide a friendly prompt in UserPromptMessage asking for the country name.
            If the user asks about anything other than country information, refuse politely and only return a prompt asking for the country name (Success=false).
            
            Use standard 3-letter ISO country codes (e.g., "USA", "GBR", "FRA").
            """;
        

        return agentProvider.GetAgent(aiModel, instructions);
    }
}
