using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Models;

namespace MicrosoftAgentFramework.Agent.Executors;

public class CountryExtractorExecutor(AgentRegistry agentRegistry) 
    : ReflectingExecutor<CountryExtractorExecutor>("CountryExtractor"), 
      IMessageHandler<string, ExtractCountryNameResponse>
{
    public async ValueTask<ExtractCountryNameResponse> HandleAsync(
        string message, 
        IWorkflowContext context, 
        CancellationToken cancellationToken)
    {
        var agent = agentRegistry.Get(AgentName.CountryExtractor);
        
        var response = await agent.RunAsync<ExtractCountryNameResponse>(
            message: message, 
            cancellationToken: cancellationToken);
        
        return response.Result;
    }
}
