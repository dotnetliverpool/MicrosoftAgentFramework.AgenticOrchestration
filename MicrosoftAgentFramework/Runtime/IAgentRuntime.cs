using MicrosoftAgentFramework.Agent;

namespace MicrosoftAgentFramework.Runtime;

public interface IAgentRuntime
{
    IThreadBoundAgentRuntime WithAgent(AgentName agentName, Dictionary<string, object>? context = null);
}
