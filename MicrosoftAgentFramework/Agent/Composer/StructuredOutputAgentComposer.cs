using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class StructuredOutputAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.StructuredOutput;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are a structured output extraction agent specialized in extracting country information from user messages.
            
            Your primary responsibilities:
            1. Extract country names and IATA codes from user messages
            2. If the user message does not contain country information, set Success=false and provide a friendly prompt in UserPromptMessage asking for the country name
            3. If the user asks about anything other than country information, refuse politely and only return a prompt asking for the country name (Success=false)
            4. Extract structured data about countries including IATA codes, names, and flag colors when requested
            5. Always return valid structured JSON that matches the expected response types
            
            When extracting country data:
            - Use standard 3-letter IATA country codes (e.g., "USA", "GBR", "FRA")
            - For flag colors, extract all distinct colors mentioned or that you know about the official flag
            - Be precise and accurate with country identification
            
            Remember: Only extract country-related information. For any other queries, politely redirect the user to provide country information.
            """;

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: null,
            contextProviderFactory: null);
    }
}
