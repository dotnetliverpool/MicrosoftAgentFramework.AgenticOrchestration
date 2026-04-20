using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Runtime;

public class InMemoryThreadManager : IThreadManager
{
    private readonly ConcurrentDictionary<string, AgentSession> _sessions = new();

    public async Task<AgentSession> GetOrCreateAsync(
        string sessionId,
        Func<JsonElement, CancellationToken, Task<AgentSession>> deserializeSessionFactory,
        Func<CancellationToken, Task<AgentSession>> sessionFactory,
        CancellationToken ct = default)
    {
        var key = string.IsNullOrWhiteSpace(sessionId) ? "default-session" : sessionId;
        if (_sessions.TryGetValue(key, out var existingSession))
            return existingSession;

        var createdSession = await sessionFactory(ct);
        return _sessions.GetOrAdd(key, createdSession);
    }

    public Task SaveAsync(
        string sessionId,
        AgentSession session,
        Func<AgentSession, CancellationToken, ValueTask<JsonElement>> serializeSessionFactory,
        CancellationToken ct = default)
    {
        // In-memory manager keeps full AgentSession objects in process only.
        return Task.CompletedTask;
    }
}
