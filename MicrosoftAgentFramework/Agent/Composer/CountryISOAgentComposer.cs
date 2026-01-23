using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class CountryISOAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.CountryISOExpert;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.High,
            Name = Name.ToString()
        };

        const string instructions = """
            Extract country name and ISO 3166-1 alpha-3 code from any message containing a geographic reference.
            
            Extract from: country names ("Portugal", "USA"), cities ("Paris" → France), regions ("California" → USA), landmarks ("Eiffel Tower" → France).
            
            Return Success=true with CountryName and ISOCode when a geographic reference exists.
            Return Success=false with a creative, friendly UserPromptMessage that guides the user to provide a country name or location. Make the message contextual and helpful based on their query.
            
            Examples:
            - "flag of portugal" → Success=true, CountryName="Portugal", ISOCode="PRT"
            - "paris weather" → Success=true, CountryName="France", ISOCode="FRA"
            - "what is the weather" → Success=false, UserPromptMessage="I'd love to help! Could you tell me which country or city you're interested in?"
            """;

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: null,
            contextProviderFactory: null);
    }
}
