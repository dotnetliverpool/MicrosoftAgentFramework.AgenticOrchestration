using Microsoft.Agents.AI.Workflows;
using MicrosoftAgentFramework.Models;

namespace MicrosoftAgentFramework.Agent.Executors;

public partial class CountryCacheCheckerExecutor()
    : Executor("CountryCacheChecker")
{
    public static readonly List<Country> CountriesCache = new();

    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.ConfigureRoutes(routeBuilder =>
            routeBuilder.AddHandler<ExtractCountryNameResponse, object>(HandleAsync));
        return protocolBuilder;
    }

    [MessageHandler]
    private ValueTask<object> HandleAsync(
        ExtractCountryNameResponse extractResponse,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        // Check if country exists in cache
        var existingCountry = CountriesCache.FirstOrDefault(x => 
            string.Equals(x.ISOCode, extractResponse.ISOCode, StringComparison.OrdinalIgnoreCase));

        // Return Country if found, otherwise return ExtractCountryNameResponse for enrichment
        object result = existingCountry != null ? (object)existingCountry : extractResponse;
        
        return ValueTask.FromResult(result);
    }
}
