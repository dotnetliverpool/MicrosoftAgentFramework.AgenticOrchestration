using Microsoft.Agents.AI;

namespace MicrosoftAgentFramework.Agent.Composer;

public interface IAgentComposer
{
    public AgentName Name { get; }
    public ChatClientAgent Get(Dictionary<string, object>? context = null);
}