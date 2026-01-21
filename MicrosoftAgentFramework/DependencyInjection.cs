using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Agent.Composer;
using MicrosoftAgentFramework.Configuration;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;

namespace MicrosoftAgentFramework;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Configuration
        services.Configure<OpenAIConfig>(configuration.GetSection("OpenAI"));

        // Register Azure OpenAI Client
        var openAiConfig = configuration.GetSection("OpenAI").Get<OpenAIConfig>() 
            ?? throw new InvalidOperationException("OpenAI configuration is missing");
        
        services.AddSingleton<AzureOpenAIClient>(sp =>
        {
            return new AzureOpenAIClient(
                new Uri(openAiConfig.Endpoint),
                new AzureKeyCredential(openAiConfig.ApiKey));
        });

        // Register Services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddHttpClient<CountriesNowApiClient>();
        services.AddSingleton<AgentRegistry>();

        // Register Agent Provider
        services.AddSingleton<IAgentProvider, AzureOpenAiAgentProvider>();

        // Register Agent Implementations (keyed services)
        services.AddKeyedSingleton<IAgentImplementation, AzureOpenAiChatClientImplementation>(AgentClient.ChatClient);
        services.AddKeyedSingleton<IAgentImplementation, AzureOpenAiResponseClientImplementation>(AgentClient.ResponseClient);

        // Register Agent Composers (keyed services)
        services.AddKeyedSingleton<IAgentComposer, StructuredOutputAgentComposer>(AgentName.StructuredOutput);
        services.AddKeyedSingleton<IAgentComposer, AgentToolAgentComposer>(AgentName.AgentWithTools);
        services.AddKeyedSingleton<IAgentComposer, AgentAsToolComposer>(AgentName.AgentAsTool);
        services.AddKeyedSingleton<IAgentComposer, CountryExtractorComposer>(AgentName.CountryExtractor);
        services.AddKeyedSingleton<IAgentComposer, CountryDataEnricherComposer>(AgentName.CountryDataEnricher);
        services.AddKeyedSingleton<IAgentComposer, OrchestratorAgentComposer>(AgentName.OrchestratorAgent);
        services.AddKeyedSingleton<IAgentComposer, LocationAgentComposer>(AgentName.LocationAgent);
        services.AddKeyedSingleton<IAgentComposer, WeatherAgentComposer>(AgentName.WeatherAgent);
        services.AddKeyedSingleton<IAgentComposer, TranslatorAgentComposer>(AgentName.TranslatorAgent);

        return services;
    }
}
