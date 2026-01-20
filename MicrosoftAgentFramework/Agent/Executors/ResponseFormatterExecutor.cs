using System.Text.Json;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using MicrosoftAgentFramework.Models;

namespace MicrosoftAgentFramework.Agent.Executors;

public class ResponseFormatterExecutor(string responseLanguage) 
    : ReflectingExecutor<ResponseFormatterExecutor>("ResponseFormatter"), 
      IMessageHandler<Country, string>
{
    public ValueTask<string> HandleAsync(
        Country country, 
        IWorkflowContext context, 
        CancellationToken cancellationToken)
    {
        var response = new
        {
            country = country,
            language = responseLanguage,
            formattedAt = DateTime.UtcNow
        };
        
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        return ValueTask.FromResult(json);
    }
}
