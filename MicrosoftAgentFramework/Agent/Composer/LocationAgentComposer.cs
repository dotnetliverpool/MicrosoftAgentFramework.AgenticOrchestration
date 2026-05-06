using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Agent.Tools;

namespace MicrosoftAgentFramework.Agent.Composer;

public class LocationAgentComposer(IAgentProvider agentProvider, CountryTools countryTools) : IAgentComposer
{
    public AgentName Name => AgentName.LocationAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are a location and country data expert.
            You answer questions about countries, cities, populations, currencies, and geographical data.
            Provide detailed, helpful responses about any country-related queries.
            """;

        List<AITool> tools = new()
        {
            countryTools.CityPopulation,
            countryTools.CountryPopulation,
            countryTools.CountryCurrency,
            countryTools.CountryPosition
        };

        return agentProvider.GetAgent(aiModel, instructions, tools);
    }
}
