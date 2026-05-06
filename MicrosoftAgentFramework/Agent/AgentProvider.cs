using Azure.AI.OpenAI;
using System.Linq;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MicrosoftAgentFramework.Configuration;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;

namespace MicrosoftAgentFramework.Agent;

public class AzureOpenAiAgentProvider(
    AzureOpenAIClient azureOpenAiClient, IServiceProvider serviceProvider, IOptions<OpenAIConfig> options) : IAgentProvider
{
    private readonly OpenAIConfig _options = options.Value;
    public ChatClientAgent GetAgent(
        AiModel aiModel, 
        string instructions, 
        List<AITool>? tools,
        List<AIContextProvider>? contextProviders,
        IChatReducer? chatReducer)
    {
        var implementation = serviceProvider.GetKeyedService<IAgentImplementation>(aiModel.ClientType)
                             ?? throw new NotSupportedException($"No implementation registered for {aiModel.ClientType.ToString()}");
        aiModel.ModelName = _options.DeploymentName;
        
        return implementation.Create(azureOpenAiClient, aiModel, instructions, tools, contextProviders, chatReducer);
    }
}

public class AzureOpenAiChatClientImplementation(ILoggerFactory loggerFactory) : IAgentImplementation
{
    public AgentClient ClientStrategy { get; } = AgentClient.ChatClient;
    public ChatClientAgent Create(
        OpenAIClient client, 
        AiModel model, 
        string instructions, 
        List<AITool>? tools,
        List<AIContextProvider>? contextProviders,
        IChatReducer? chatReducer)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        
        var agentOptions = new ChatClientAgentOptions
        {
            Name = model.Name,
            ChatOptions = new ChatOptions
            {
                Instructions = instructions
            }
        };

        if (tools?.Count > 0)
        {
            agentOptions.ChatOptions.Tools = tools;
            agentOptions.ChatOptions.AllowMultipleToolCalls = true;
            agentOptions.ChatOptions.ToolMode = ChatToolMode.Auto;
        }
        
        contextProviders ??= [];
        agentOptions.AIContextProviders = contextProviders;
        
        return client
            .GetChatClient(model.ModelName)
            .AsAIAgent(options: agentOptions, clientFactory: (chatClient) =>
            {
                // Default reducer keeps long conversations within model limits while allowing per-agent overrides.
#pragma warning disable MEAI001
                var reducerToUse = chatReducer ??
                                   new SummarizingChatReducer(chatClient, targetCount: 1, threshold: 8);
#pragma warning restore MEAI001
                return chatClient
                    .AsBuilder()
                    .Use(async (messages, options, next, ct) =>
                    {
                        var toolCalls = messages
                            .SelectMany(message => message.Contents)
                            .OfType<FunctionCallContent>();

                        if (toolCalls.Any())
                        {
                            var log = loggerFactory.CreateLogger("ToolCall");
                            foreach (var call in toolCalls)
                            {
                                log.LogInformation(
                                    "[DEMO] ToolCall   {FunctionName}  args: {Arguments}",
                                    call.Name,
                                    call.Arguments);
                            }
                        }

                        await next(messages, options, ct);
                    })
                    .UseChatReducer(reducerToUse)
                    .Build();
            });
    }
}

public class AzureOpenAiResponseClientImplementation(IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : IAgentImplementation
{
    public AgentClient ClientStrategy { get; } = AgentClient.ResponseClient;
    public ChatClientAgent Create(
        OpenAIClient client, 
        AiModel model, 
        string instructions, 
        List<AITool>? tools,
        List<AIContextProvider>? contextProviders,
        IChatReducer? chatReducer)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        // ResponseClient currently lacks the chat pipeline hooks used for AIContextProviders/reducers.
        // Keep signature parity at abstraction boundary; values are intentionally ignored here.
#pragma warning disable OPENAI001
        return client
            .GetResponsesClient()
            .AsAIAgent(instructions: instructions, tools: tools ?? null,
                name: model.Name, description: model.Description, loggerFactory: loggerFactory,
                services: serviceProvider);
#pragma warning restore OPENAI001 
    }
}
