using Microsoft.Agents.AI.Workflows;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Models;
using MicrosoftAgentFramework.Runtime;

namespace MicrosoftAgentFramework.Agent.Executors;

public partial class CountryDataEnricherExecutor(IAgentRuntime agentRuntime)
    : Executor("CountryDataEnricher")
{
    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.ConfigureRoutes(routeBuilder =>
            routeBuilder.AddHandler<ExtractCountryNameResponse, Country>(HandleAsync));
        return protocolBuilder;
    }

    [MessageHandler]
    private async ValueTask<Country> HandleAsync(
        ExtractCountryNameResponse extractResponse,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var message = $"what are the colors in the flag of {extractResponse.ISOCode}, {extractResponse.CountryName}";

        var response = await agentRuntime
            .WithAgent(AgentName.CountryDataEnricher)
            .RunAsync<Country>(message, cancellationToken);

        // Add to cache
        CountryCacheCheckerExecutor.CountriesCache.Add(response.Result!);

        return response.Result!;
    }
}
