using Microsoft.Agents.AI;
using MicrosoftAgentFramework.Agent.Composer;

namespace MicrosoftAgentFramework.Agent;

public class AgentRegistry(IServiceProvider serviceProvider)
{
    public ChatClientAgent Get(AgentName agentName, Dictionary<string, object>? context = null)
    {
        var composer = serviceProvider.GetKeyedService<IAgentComposer>(agentName) ??
                       throw new Exception("This agent has not been configured");
        return composer.Get(context);
    }
}



