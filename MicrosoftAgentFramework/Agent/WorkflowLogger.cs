using Microsoft.Agents.AI.Workflows;
using MicrosoftAgentFramework.Runtime;

namespace MicrosoftAgentFramework.Agent;

public static class WorkflowLogger
{
    public static void Log(WorkflowEvent evt, ILogger logger)
    {
        switch (evt)
        {
            case WorkflowStartedEvent started:
                logger.LogInformation("[Workflow] Started with input: {Input}", started.Data);
                break;
            case ExecutorInvokedEvent invoked:
                if (TryGetHandoffRoute(invoked.Data, out var source, out var target))
                {
                    logger.LogInformation(
                        "[Handoff] Invoked from: {Source} to {Target}",
                        source,
                        target);
                }
                break;
            case AgentResponseUpdateEvent update:
                break;
            case AgentResponseEvent response:
                var usage = UsageInfo.From(response.Response);
                logger.LogInformation(
                    "[{ExecutorId}] Response - in: {Prompt} out: {Completion} total: {Total}",
                    response.ExecutorId,
                    usage.PromptTokens ?? 0,
                    usage.CompletionTokens ?? 0,
                    usage.TotalTokens ?? 0);
                break;
            case ExecutorCompletedEvent completed:
                logger.LogInformation("[{ExecutorId}] Completed", completed.ExecutorId);
                break;
            case ExecutorFailedEvent failed:
                logger.LogError(failed.Data as Exception, "[{ExecutorId}] Failed", failed.ExecutorId);
                break;
            case WorkflowOutputEvent output:
                var sourceId = string.IsNullOrWhiteSpace(output.SourceId) ? "UnknownSource" : output.SourceId;
                logger.LogInformation("[Handoff] {SourceId} -> {ExecutorId}", sourceId, output.ExecutorId);
                break;
            case SubworkflowErrorEvent subworkflowError:
                logger.LogError("[Subworkflow:{SubworkflowId}] Error", subworkflowError.SubworkflowId);
                break;
            case SubworkflowWarningEvent subworkflowWarning:
                logger.LogWarning("[Subworkflow:{SubworkflowId}] Warning", subworkflowWarning.SubWorkflowId);
                break;
            case WorkflowErrorEvent workflowError:
                logger.LogError(workflowError.Exception, "[Workflow] Error");
                break;
            case WorkflowWarningEvent warning:
                logger.LogWarning("[Workflow] Warning: {WarningData}", warning.Data);
                break;
        }
    }

    private static bool TryGetHandoffRoute(object? data, out string source, out string target)
    {
        source = "UnknownSource";
        target = "UnknownTarget";

        if (data is null)
        {
            return false;
        }

        var dataType = data.GetType();
        if (dataType.FullName != "Microsoft.Agents.AI.Workflows.Specialized.HandoffState")
        {
            return false;
        }

        source = dataType.GetProperty("PreviousAgentId")?.GetValue(data) as string ?? "UnknownSource";
        target = dataType.GetProperty("RequestedHandoffTargetAgentId")?.GetValue(data) as string ?? "UnknownTarget";
        return true;
    }
}
