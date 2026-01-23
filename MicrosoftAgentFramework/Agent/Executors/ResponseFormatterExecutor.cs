using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using MicrosoftAgentFramework.Models;

namespace MicrosoftAgentFramework.Agent.Executors;

public class ResponseFormatterExecutor(
    AgentRegistry agentRegistry,
    string responseLanguage) 
    : ReflectingExecutor<ResponseFormatterExecutor>("ResponseFormatter"), 
      IMessageHandler<Country, string>,
      IMessageHandler<ExtractCountryNameResponse, string>
{
    public async ValueTask<string> HandleAsync(
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

    public async ValueTask<string> HandleAsync(
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
        var agent = agentRegistry.Get(AgentName.ResponseTranslator, new Dictionary<string, object>
        {
            { "responseLanguage", responseLanguage }
        });

        var message = $"Translate this response: {jsonData}";
        var response = await agent.RunAsync(message: message, cancellationToken: cancellationToken);
        
        return response.Text;
    }
}
