using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using MicrosoftAgentFramework.Models;

namespace MicrosoftAgentFramework.Agent.Executors;

public class CountryCacheCheckerExecutor() 
    : ReflectingExecutor<CountryCacheCheckerExecutor>("CountryCacheChecker"), 
      IMessageHandler<ExtractCountryNameResponse, object>
{
    public static readonly List<Country> CountriesCache = new();

    public ValueTask<object> HandleAsync(
        ExtractCountryNameResponse extractResponse, 
        IWorkflowContext context, 
        CancellationToken cancellationToken)
    {
        // Check if country exists in cache
        var existingCountry = CountriesCache.FirstOrDefault(x => 
            string.Equals(x.IATACode, extractResponse.IATACode, StringComparison.OrdinalIgnoreCase));

        // Return Country if found, otherwise return ExtractCountryNameResponse for enrichment
        object result = existingCountry != null ? (object)existingCountry : extractResponse;
        
        return ValueTask.FromResult(result);
    }
}
