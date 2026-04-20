using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Hosting;

namespace MicrosoftAgentFramework.Runtime;

public class JsonDocumentThreadManager : IThreadManager
{
    private readonly string _sessionsPath;
    private readonly ConcurrentDictionary<string, AgentSession> _sessions = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public JsonDocumentThreadManager(IHostEnvironment hostEnvironment)
    {
        _sessionsPath = Path.Combine(hostEnvironment.ContentRootPath, "App_Data", "sessions");
        Directory.CreateDirectory(_sessionsPath);
    }

    public async Task<AgentSession> GetOrCreateAsync(
        string sessionId,
        Func<JsonElement, CancellationToken, Task<AgentSession>> deserializeSessionFactory,
        Func<CancellationToken, Task<AgentSession>> sessionFactory,
        CancellationToken ct = default)
    {
        var key = string.IsNullOrWhiteSpace(sessionId) ? "default-session" : sessionId;
        if (_sessions.TryGetValue(key, out var existingSession))
            return existingSession;

        var gate = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            if (_sessions.TryGetValue(key, out existingSession))
                return existingSession;

            var path = GetSessionPath(key);
            AgentSession loadedOrCreated;
            if (File.Exists(path))
            {
                await using var stream = File.OpenRead(path);
                var json = await JsonSerializer.DeserializeAsync<JsonElement>(stream, cancellationToken: ct);
                loadedOrCreated = await deserializeSessionFactory(json, ct);
            }
            else
            {
                loadedOrCreated = await sessionFactory(ct);
            }

            _sessions[key] = loadedOrCreated;
            return loadedOrCreated;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(
        string sessionId,
        AgentSession session,
        Func<AgentSession, CancellationToken, ValueTask<JsonElement>> serializeSessionFactory,
        CancellationToken ct = default)
    {
        var key = string.IsNullOrWhiteSpace(sessionId) ? "default-session" : sessionId;
        var gate = _locks.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            _sessions[key] = session;
            var json = await serializeSessionFactory(session, ct);
            var path = GetSessionPath(key);
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, json, cancellationToken: ct);
        }
        finally
        {
            gate.Release();
        }
    }

    private string GetSessionPath(string sessionId)
    {
        var safeName = string.Concat(sessionId.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch));
        return Path.Combine(_sessionsPath, $"{safeName}.json");
    }
}
