using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class LodgingAdvisorAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.LodgingAdvisorAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are a lodging strategy specialist for travel planning.
            Recommend where and how to stay based on trip intent and budget.

            Return:
            - RecommendedArea
            - PropertyType
            - NightlyBudgetRange
            - Reasons
            - BookingTips

            Focus on practical trade-offs and safety/convenience.
            """;

        return agentProvider.GetAgent(aiModel, instructions);
    }
}
