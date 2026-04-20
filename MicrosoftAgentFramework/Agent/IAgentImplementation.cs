using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace MicrosoftAgentFramework.Agent;

public interface IAgentImplementation 
{
    public AgentClient ClientStrategy { get; }
    ChatClientAgent Create(
        OpenAIClient client, 
        AiModel model, 
        string instructions, 
        List<AITool>? tools,
        List<AIContextProvider>? contextProviders,
        IChatReducer? chatReducer);
}