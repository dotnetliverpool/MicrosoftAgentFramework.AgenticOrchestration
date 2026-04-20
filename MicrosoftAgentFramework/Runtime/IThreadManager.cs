using Microsoft.Agents.AI;
using System.Text.Json;

namespace MicrosoftAgentFramework.Runtime;

public interface IThreadManager
{
    Task<AgentSession> GetOrCreateAsync(
        string sessionId,
        Func<JsonElement, CancellationToken, Task<AgentSession>> deserializeSessionFactory,
        Func<CancellationToken, Task<AgentSession>> sessionFactory,
        CancellationToken ct = default);

    Task SaveAsync(
        string sessionId,
        AgentSession session,
        Func<AgentSession, CancellationToken, ValueTask<JsonElement>> serializeSessionFactory,
        CancellationToken ct = default);
}
