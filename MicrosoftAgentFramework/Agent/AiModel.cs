namespace MicrosoftAgentFramework.Agent;

public class AiModel
{
    public AgentReasoningEffortLevel ReasoningEffortLevel { get; set; } = AgentReasoningEffortLevel.Medium;
    public string ModelName { get; set; } = string.Empty;
    public AgentClient ClientType { get; set; } = AgentClient.ChatClient;
    
    public string? Name { get; set; } 
    public string? Description { get; set; }

}