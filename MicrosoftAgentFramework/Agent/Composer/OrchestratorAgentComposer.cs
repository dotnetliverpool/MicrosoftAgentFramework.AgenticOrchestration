using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class OrchestratorAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.OrchestratorAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are an orchestrator agent that coordinates between specialized agents.
            Analyze the user's question and determine which specialist agent can best answer it:
            - LocationAgent: For questions about countries, cities, populations, currencies, geographical data
            - WeatherAgent: For questions about weather, forecasts, or weather conditions
            
            If the question requires both location and weather information, coordinate with both agents.
            After receiving complete responses from specialist agents, hand off to the TranslatorAgent to present the final answer in the requested language.
            """;

        return agentProvider.GetAgent(aiModel, instructions);
    }
}
