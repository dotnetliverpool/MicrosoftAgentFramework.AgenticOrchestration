using System.Diagnostics;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using MicrosoftAgentFramework.Agent;

namespace MicrosoftAgentFramework.Runtime;

public class AgentRuntime(AgentRegistry registry, IThreadManager threadManager, ILogger<AgentRuntime> logger) : IAgentRuntime
{
    public IThreadBoundAgentRuntime WithAgent(AgentName agentName, Dictionary<string, object>? context = null)
    {
        return new ThreadBoundAgentRuntime(registry, threadManager, logger, agentName, context);
    }

    private sealed class ThreadBoundAgentRuntime(
        AgentRegistry registry,
        IThreadManager threadManager,
        ILogger logger,
        AgentName agentName,
        Dictionary<string, object>? context) : IThreadBoundAgentRuntime
    {
        private const string DefaultSessionId = "default-session";
        private string _sessionId = DefaultSessionId;

        public IThreadBoundAgentRuntime ForSession(string sessionId)
        {
            _sessionId = string.IsNullOrWhiteSpace(sessionId) ? DefaultSessionId : sessionId;
            return this;
        }

        public async Task<AgentRunResult<T>> RunAsync<T>(string message, CancellationToken ct = default)
        {
            var execution = await ExecuteAsync(
                (agent, session, token) => agent.RunAsync<T>(message: message, session: session, cancellationToken: token),
                ct);

            return new AgentRunResult<T>
            {
                SessionId = _sessionId,
                ThreadId = _sessionId,
                AgentName = agentName.ToString(),
                PromptTokens = execution.Usage.PromptTokens,
                CompletionTokens = execution.Usage.CompletionTokens,
                TotalTokens = execution.Usage.TotalTokens,
                DurationMs = execution.DurationMs,
                Result = execution.Response.Result
            };
        }

        public async Task<AgentRunResult> RunAsync(string message, CancellationToken ct = default)
        {
            var execution = await ExecuteAsync(
                (agent, session, token) => agent.RunAsync(message: message, session: session, cancellationToken: token),
                ct);

            return new AgentRunResult
            {
                SessionId = _sessionId,
                ThreadId = _sessionId,
                AgentName = agentName.ToString(),
                PromptTokens = execution.Usage.PromptTokens,
                CompletionTokens = execution.Usage.CompletionTokens,
                TotalTokens = execution.Usage.TotalTokens,
                DurationMs = execution.DurationMs,
                Text = execution.Response.Text
            };
        }

        private async Task<ExecutionContext<TResponse>> ExecuteAsync<TResponse>(
            Func<ChatClientAgent, AgentSession, CancellationToken, Task<TResponse>> invoke,
            CancellationToken ct)
        {
            // Runtime orchestration boundary: resolve agent, hydrate session, execute, persist, then emit telemetry.
            var agent = registry.Get(agentName, context);
            var session = await threadManager.GetOrCreateAsync(
                _sessionId,
                (serializedState, cancellationToken) => agent.DeserializeSessionAsync(serializedState, cancellationToken: cancellationToken).AsTask(),
                cancellationToken => agent.CreateSessionAsync(cancellationToken).AsTask(),
                ct);

            var stopwatch = Stopwatch.StartNew();
            var response = await invoke(agent, session, ct);
            stopwatch.Stop();

            await threadManager.SaveAsync(
                _sessionId,
                session,
                (agentSession, cancellationToken) => agent.SerializeSessionAsync(agentSession, cancellationToken: cancellationToken),
                ct);

            var usage = UsageInfo.From(response!);

            logger.LogInformation("[DEMO] --------------------------------------------");
            logger.LogInformation("[DEMO] Agent      {AgentName}", agentName.ToString());
            logger.LogInformation("[DEMO] Session    {SessionId}", _sessionId);
            logger.LogInformation(
                "[DEMO] Tokens     in: {Prompt}  out: {Completion}  total: {Total}",
                usage.PromptTokens ?? 0,
                usage.CompletionTokens ?? 0,
                usage.TotalTokens ?? 0);
            logger.LogInformation("[DEMO] Duration   {Duration} ms", (long)stopwatch.Elapsed.TotalMilliseconds);
            logger.LogInformation("[DEMO] --------------------------------------------");

            return new ExecutionContext<TResponse>(response, usage, stopwatch.Elapsed.TotalMilliseconds);
        }

        private sealed record ExecutionContext<TResponse>(TResponse Response, UsageInfo Usage, double DurationMs);
    }
}
