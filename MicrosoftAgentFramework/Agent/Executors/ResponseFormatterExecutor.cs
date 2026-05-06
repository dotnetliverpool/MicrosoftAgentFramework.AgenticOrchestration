using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Models;
using MicrosoftAgentFramework.Runtime;

namespace MicrosoftAgentFramework.Agent.Executors;

public partial class ResponseFormatterExecutor(
    IAgentRuntime agentRuntime,
    string responseLanguage)
    : Executor("ResponseFormatter")
{
    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        protocolBuilder.ConfigureRoutes(routeBuilder =>
        {
            routeBuilder.AddHandler<Country, string>(
                (country, ctx, ct) => HandleAsync(country, ctx, ct));
            routeBuilder.AddHandler<ExtractCountryNameResponse, string>(
                (errorResponse, ctx, ct) => HandleAsync(errorResponse, ctx, ct));
        });
        return protocolBuilder;
    }

    [MessageHandler]
    private async ValueTask<string> HandleAsync(
        Country country,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(country, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        return await TranslateResponseAsync(json, cancellationToken);
    }

    [MessageHandler]
    private async ValueTask<string> HandleAsync(
        ExtractCountryNameResponse errorResponse,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        return await TranslateResponseAsync(json, cancellationToken);
    }

    private async ValueTask<string> TranslateResponseAsync(string jsonData, CancellationToken cancellationToken)
    {
        var message = $"Narrate this country data for the user: {jsonData}";
        var response = await agentRuntime
            .WithAgent(
                AgentName.CountryDataNarratorAgent,
                new Dictionary<string, object>
                {
                    { "responseLanguage", responseLanguage }
                })
            .RunAsync(message, cancellationToken);

        return response.Text ?? string.Empty;
    }
}
