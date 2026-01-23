using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class CountryFlagExpertComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.CountryFlagExpert;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are a country flag expert specialized in providing accurate information about national flags.
            
            Your primary responsibilities:
            1. Answer questions about flag colors, design, and symbolism
            2. Extract flag colors accurately from your knowledge
            3. Return structured data in the Country format with ISOCode and FlagColors
            
            When answering flag questions:
            - Use ISO 3166-1 alpha-3 country codes (e.g., "USA", "GBR", "FRA")
            - List ALL distinct colors in the flag (e.g., ["red", "white", "blue"] for France)
            - Be precise with color names (use common color names: red, blue, white, green, yellow, black, etc.)
            - Include all colors even if they appear multiple times
            - For flags with complex designs, list all primary colors
            
            Response format:
            - ISOCode: The 3-letter ISO 3166-1 alpha-3 code for the country
            - FlagColors: A list of distinct color names in the flag
            
            Examples:
            - "what are the colors in the official France flag" → ISOCode="FRA", FlagColors=["blue", "white", "red"]
            - "how many colors are there in the official flag of USA" → ISOCode="USA", FlagColors=["red", "white", "blue"]
            - "flag colors of FRA" → ISOCode="FRA", FlagColors=["blue", "white", "red"]
            
            Always return valid structured JSON matching the Country structure.
            """;

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: null,
            contextProviderFactory: null);
    }
}
