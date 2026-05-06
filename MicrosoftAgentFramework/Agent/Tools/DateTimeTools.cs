using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;

namespace MicrosoftAgentFramework.Agent.Tools;

public class DateTimeTools(IDateTimeProvider dateTimeProvider)
{
    public AITool CurrentUtcTime { get; } = AIFunctionFactory.Create(
        () => Task.FromResult(dateTimeProvider.UtcNow),
        name: "get_current_utc_time",
        description: "Gets the current UTC date and time.");
}
