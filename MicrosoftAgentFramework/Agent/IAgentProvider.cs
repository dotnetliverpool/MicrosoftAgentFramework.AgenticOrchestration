using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;


namespace MicrosoftAgentFramework.Agent;

public interface IAgentProvider
{
    public ChatClientAgent GetAgent(
        AiModel aiModel, 
        string instructions, 
        List<AITool>? tools = null,
        Func<ChatClientAgentOptions.AIContextProviderFactoryContext, AIContextProvider>? contextProviderFactory = null);
}