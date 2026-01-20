using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.Extensions.AI;

namespace MicrosoftAgentFramework.Agent.Middleware;

public static class FunctionCallLoggingMiddleware
{
    public static Func<AIAgent, FunctionInvocationContext, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>>, CancellationToken, ValueTask<object?>> Create(ILogger logger)
    {
        return async (callingAgent, context, next, cancellationToken) =>
        {
            StringBuilder functionCallDetails = new();
            functionCallDetails.Append($"- Tool Call: '{context.Function.Name}'");
            
            if (context.Arguments.Count > 0)
            {
                functionCallDetails.Append($" (Args: {string.Join(", ", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))})");
            }

            logger.LogInformation(functionCallDetails.ToString());

            return await next(context, cancellationToken);
        };
    }
}
