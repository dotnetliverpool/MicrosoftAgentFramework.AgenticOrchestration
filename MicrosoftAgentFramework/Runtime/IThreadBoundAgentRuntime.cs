namespace MicrosoftAgentFramework.Runtime;

public interface IThreadBoundAgentRuntime
{
    IThreadBoundAgentRuntime ForSession(string sessionId);

    Task<AgentRunResult<T>> RunAsync<T>(string message, CancellationToken ct = default);

    Task<AgentRunResult> RunAsync(string message, CancellationToken ct = default);
}
