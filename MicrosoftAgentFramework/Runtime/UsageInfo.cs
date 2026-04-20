namespace MicrosoftAgentFramework.Runtime;

internal sealed class UsageInfo
{
    private static readonly string[] PromptNames = ["PromptTokens", "InputTokenCount", "InputTokens"];
    private static readonly string[] CompletionNames = ["CompletionTokens", "OutputTokenCount", "OutputTokens"];
    private static readonly string[] TotalNames = ["TotalTokens", "TotalTokenCount"];
    private static readonly string[] UsageObjectNames = ["Usage", "TokenUsage", "ModelUsage"];

    public int? PromptTokens { get; init; }
    public int? CompletionTokens { get; init; }
    public int? TotalTokens { get; init; }

    public static UsageInfo From(object response)
    {
        var prompt = ReadInt(response, PromptNames);
        var completion = ReadInt(response, CompletionNames);
        var total = ReadInt(response, TotalNames);

        if (prompt is null && completion is null && total is null)
        {
            var usageObject = ReadObject(response, UsageObjectNames);
            prompt = ReadInt(usageObject, PromptNames);
            completion = ReadInt(usageObject, CompletionNames);
            total = ReadInt(usageObject, TotalNames);
        }

        return new UsageInfo
        {
            PromptTokens = prompt,
            CompletionTokens = completion,
            TotalTokens = total ?? (prompt is not null && completion is not null ? prompt + completion : null)
        };
    }

    private static object? ReadObject(object? source, string[] propertyNames)
    {
        if (source is null)
            return null;

        var sourceType = source.GetType();
        foreach (var name in propertyNames)
        {
            if (sourceType.GetProperty(name)?.GetValue(source) is { } value)
                return value;
        }

        return null;
    }

    private static int? ReadInt(object? source, string[] propertyNames)
    {
        if (source is null)
            return null;

        var sourceType = source.GetType();
        foreach (var name in propertyNames)
        {
            var value = sourceType.GetProperty(name)?.GetValue(source);
            if (value is null)
                continue;

            return value switch
            {
                int i => i,
                long l => Convert.ToInt32(l),
                short s => s,
                byte b => b,
                string str when int.TryParse(str, out var parsed) => parsed,
                _ => null
            };
        }

        return null;
    }
}
