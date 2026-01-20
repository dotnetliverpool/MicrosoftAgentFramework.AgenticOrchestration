using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MicrosoftAgentFramework.Agent.Middleware;
using MicrosoftAgentFramework.Configuration;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;

namespace MicrosoftAgentFramework.Agent;

public class AzureOpenAiAgentProvider(AzureOpenAIClient azureOpenAiClient, IServiceProvider serviceProvider, IOptions<OpenAIConfig> options, ILoggerFactory loggerFactory) : IAgentProvider
{
    private readonly OpenAIConfig _options = options.Value;
    public ChatClientAgent GetAgent(
        AiModel aiModel, 
        string instructions, 
        List<AITool>? tools = null,
        Func<ChatClientAgentOptions.AIContextProviderFactoryContext, AIContextProvider>? contextProviderFactory = null)
    {
        var implementation = serviceProvider.GetKeyedService<IAgentImplementation>(aiModel.ClientType)
                             ?? throw new NotSupportedException($"No implementation registered for {aiModel.ClientType.ToString()}");
        aiModel.ModelName = _options.DeploymentName;
        
        var agent = implementation.Create(azureOpenAiClient, aiModel, instructions, tools, contextProviderFactory);
        
        // Create logger typed to agent name
        var loggerCategory = aiModel.Name ?? "UnknownAgent";
        var logger = loggerFactory.CreateLogger(loggerCategory);
        
        // Add middleware for function call logging
        return (ChatClientAgent)agent
            .AsBuilder()
            .Use(FunctionCallLoggingMiddleware.Create(logger))
            .Build();
    }
}

public class AzureOpenAiChatClientImplementation : IAgentImplementation
{
    public AgentClient ClientStrategy { get; } = AgentClient.ChatClient;
    public ChatClientAgent Create(
        OpenAIClient client, 
        AiModel model, 
        string instructions, 
        List<AITool>? tools,
        Func<ChatClientAgentOptions.AIContextProviderFactoryContext, AIContextProvider>? contextProviderFactory = null)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        
        var agentOptions = new ChatClientAgentOptions
        {
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
        
        // Add context provider factory if provided
        if (contextProviderFactory != null)
        {
            agentOptions.AIContextProviderFactory = contextProviderFactory;
        }
        
        return client
            .GetChatClient(model.ModelName)
            .CreateAIAgent(agentOptions);
    }
}

public class AzureOpenAiResponseClientImplementation(IServiceProvider serviceProvider) : IAgentImplementation
{
    public AgentClient ClientStrategy { get; } = AgentClient.ResponseClient;
    public ChatClientAgent Create(
        OpenAIClient client, 
        AiModel model, 
        string instructions, 
        List<AITool>? tools,
        Func<ChatClientAgentOptions.AIContextProviderFactoryContext, AIContextProvider>? contextProviderFactory = null)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        // Note: ResponseClient doesn't support AIContextProviderFactory in the same way
        // If contextProviderFactory is provided for ResponseClient, it's ignored
#pragma warning disable OPENAI001
        return client
            .GetResponsesClient(model.ModelName)
            .CreateAIAgent(instructions: instructions, tools: tools ?? null,
                name: model.Name, description: model.Description,
                services: serviceProvider);
#pragma warning restore OPENAI001 
    }
}
