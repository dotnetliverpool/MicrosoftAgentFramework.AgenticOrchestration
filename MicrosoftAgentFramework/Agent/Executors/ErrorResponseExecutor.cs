using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using MicrosoftAgentFramework.Models;

namespace MicrosoftAgentFramework.Agent.Executors;

public class ErrorResponseExecutor(string responseLanguage) 
    : ReflectingExecutor<ErrorResponseExecutor>("ErrorResponse"), 
      IMessageHandler<ExtractCountryNameResponse, string>
{
    public ValueTask<string> HandleAsync(
        ExtractCountryNameResponse errorResponse, 
        IWorkflowContext context, 
        CancellationToken cancellationToken)
    {
        var response = new
        {
            error = true,
            message = errorResponse.UserPromptMessage,
            language = responseLanguage
        };
        
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        return ValueTask.FromResult(json);
    }
}
