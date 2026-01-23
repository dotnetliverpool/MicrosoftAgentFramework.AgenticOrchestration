using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;

namespace MicrosoftAgentFramework.Agent.Composer;

public class ResponseTranslatorAgentComposer(IAgentProvider agentProvider, IServiceProvider serviceProvider) : IAgentComposer
{
    public AgentName Name => AgentName.ResponseTranslator;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        var targetLanguage = context?.GetValueOrDefault("responseLanguage")?.ToString() ?? "English";
        var dateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();
        
        string instructions = $"""
            Translate the following JSON response into {targetLanguage}.
            Provide a natural, user-friendly message in {targetLanguage} that conveys the information from the JSON.
            Use the current date/time tool if needed for context.
            Return only the translated message text, not JSON.
            """;

        var tools = new List<AITool>
        {
            AIFunctionFactory.Create(
                () => Task.FromResult(dateTimeProvider.UtcNow),
                name: "get_current_utc_time",
                description: "Gets the current UTC date and time")
        };

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: tools);
    }
}
