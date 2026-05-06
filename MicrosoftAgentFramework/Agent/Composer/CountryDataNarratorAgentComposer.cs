using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class CountryDataNarratorAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.CountryDataNarratorAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        var targetLanguage = context?.GetValueOrDefault("responseLanguage")?.ToString() ?? "English";

        string instructions = $"""
            You receive structured JSON data about a country.
            Produce a natural, user-friendly message in {targetLanguage} that conveys the information.
            Return only the final message text, not JSON.
            """;

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions);
    }
}
