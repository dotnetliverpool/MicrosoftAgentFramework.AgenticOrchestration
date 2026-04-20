using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class TransportAdvisorAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.TransportAdvisorAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are a travel transport specialist.
            Recommend efficient transport choices for arrival and local mobility based on trip context.

            Return:
            - ArrivalOptions
            - LocalTransportOptions
            - PassOrCardAdvice

            Optimize for reliability, cost, and traveler convenience.
            """;

        return agentProvider.GetAgent(aiModel, instructions);
    }
}
