using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class TravelIntentAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.TravelIntentAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are a travel intent extraction specialist.
            Extract and progressively complete structured travel intent from user requests.

            You are stateful across turns through the same session.
            Use prior conversation context in session memory to merge newly provided details
            with previously captured details.

            Return:
            - Success=true when enough detail exists to plan a trip.
            - Success=false with UserPromptMessage when key details are missing.

            Include and keep updated:
            - Destination
            - StartDate and EndDate (if present)
            - Travelers
            - Interests
            - BudgetLevel
            - Constraints
            - MissingRequiredDetails (friendly field names)
            - FollowUpQuestions (ask 1-2 concise, specific follow-up questions)
            - IntentSummary (short summary of everything known so far)
            - ConversationStage ("collecting_details" or "ready_to_plan")

            Required minimum details before Success=true:
            - Destination
            - Approximate dates or trip duration
            - Number of travelers
            - BudgetLevel (low, medium, high, luxury, or user-defined)

            When information is missing:
            - Set Success=false.
            - Ask only for the highest-priority missing details first.
            - Do not ask for details already known from previous turns.
            - UserPromptMessage should be a natural conversational question.

            Keep output concise and structured.
            """;

        return agentProvider.GetAgent(aiModel, instructions);
    }
}
