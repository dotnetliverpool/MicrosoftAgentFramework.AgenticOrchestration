using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Models;

namespace MicrosoftAgentFramework.Agent.Executors;

public class CountryDataEnricherExecutor(AgentRegistry agentRegistry) 
    : ReflectingExecutor<CountryDataEnricherExecutor>("CountryDataEnricher"), 
      IMessageHandler<ExtractCountryNameResponse, Country>
{
    public async ValueTask<Country> HandleAsync(
        ExtractCountryNameResponse extractResponse, 
        IWorkflowContext context, 
        CancellationToken cancellationToken)
    {
        var agent = agentRegistry.Get(AgentName.CountryDataEnricher);
        
        var message = $"what are the colors in the flag of {extractResponse.ISOCode}, {extractResponse.CountryName}";
        
        var response = await agent.RunAsync<Country>(
            message: message, 
            cancellationToken: cancellationToken);
        
        // Add to cache
        CountryCacheCheckerExecutor.CountriesCache.Add(response.Result);
        
        return response.Result;
    }
}
