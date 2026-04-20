using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;


namespace MicrosoftAgentFramework.Agent;

public interface IAgentProvider
{
    public ChatClientAgent GetAgent(
        AiModel aiModel, 
        string instructions, 
        List<AITool>? tools = null,
        List<AIContextProvider>? contextProviders = null,
        IChatReducer? chatReducer = null);
}