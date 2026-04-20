using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public class TranslatorAgentComposer(IAgentProvider agentProvider) : IAgentComposer
{
    public AgentName Name => AgentName.TranslatorAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        var targetLanguage = context?.GetValueOrDefault("responseLanguage")?.ToString() ?? "English";
        
        string instructions = $"""
            You are a translation agent.
            Translate the provided text to {targetLanguage} while preserving the meaning and context.
            Only translate if {targetLanguage} is a valid language and different from the source language.
            If the target language is the same as the source, return the text as-is.
            Maintain professional tone and accuracy in translation.
            """;

        return agentProvider.GetAgent(aiModel, instructions);
    }
}
