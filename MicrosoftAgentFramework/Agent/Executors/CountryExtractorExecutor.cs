using Microsoft.Agents.AI.Workflows;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Models;
using MicrosoftAgentFramework.Runtime;

namespace MicrosoftAgentFramework.Agent.Executors;

public partial class CountryExtractorExecutor(IAgentRuntime agentRuntime)
    : Executor("CountryExtractor")
{
    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.ConfigureRoutes(routeBuilder =>
            routeBuilder.AddHandler<string, ExtractCountryNameResponse>(HandleAsync));
        return protocolBuilder;
    }

    [MessageHandler]
    private async ValueTask<ExtractCountryNameResponse> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var response = await agentRuntime
            .WithAgent(AgentName.CountryExtractor)
            .RunAsync<ExtractCountryNameResponse>(message, cancellationToken);

        return response.Result!;
    }
}
