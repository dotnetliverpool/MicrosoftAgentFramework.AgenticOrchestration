namespace MicrosoftAgentFramework.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
