using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;

namespace MicrosoftAgentFramework.Agent.Composer;

public class CountryDataEnricherComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.CountryDataEnricher;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are an agent specialized in enriching country data with detailed information including flag colors.
            
            Extract structured data about countries including Iso codes, names, and flag colors.
            For flag colors, extract all distinct colors mentioned or that you know about the official flag.
            Be precise and accurate with country identification.
            """;
        
        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            contextProviderFactory: null);
    }
}
