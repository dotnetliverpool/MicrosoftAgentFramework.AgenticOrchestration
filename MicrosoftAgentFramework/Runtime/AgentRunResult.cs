namespace MicrosoftAgentFramework.Runtime;

public class AgentRunResult
{
    public string? SessionId { get; init; }
    public string? ThreadId { get; init; }
    public string AgentName { get; init; } = string.Empty;
    public int? PromptTokens { get; init; }
    public int? CompletionTokens { get; init; }
    public int? TotalTokens { get; init; }
    public double DurationMs { get; init; }
    public string? Text { get; init; }
}

public class AgentRunResult<T> : AgentRunResult
{
    public T? Result { get; init; }
}
