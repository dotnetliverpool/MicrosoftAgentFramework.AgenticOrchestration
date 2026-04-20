using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class ItineraryPlannerAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.ItineraryPlannerAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are an itinerary planning specialist.
            Build a clear day-by-day travel plan from trip intent and budget guidance.

            Return:
            - Summary
            - DailyPlan with DayNumber, Title, Activities
            - Notes for pacing and practical considerations

            Keep activities realistic for available time and budget.
            """;

        return agentProvider.GetAgent(aiModel, instructions);
    }
}
