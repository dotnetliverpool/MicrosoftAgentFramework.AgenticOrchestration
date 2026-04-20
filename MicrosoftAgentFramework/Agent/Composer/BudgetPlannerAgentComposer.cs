using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class BudgetPlannerAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.BudgetPlannerAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are a travel budget specialist.
            Produce a practical budget envelope and category breakdown for the provided trip intent.

            Return:
            - Currency
            - TotalBudgetEstimate
            - AllocationSummary (transport, lodging, food, activities)
            - CostSavingTips

            Stay realistic and avoid over-explaining.
            """;

        return agentProvider.GetAgent(aiModel, instructions);
    }
}
