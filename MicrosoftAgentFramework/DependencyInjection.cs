using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Agent.Composer;
using MicrosoftAgentFramework.Configuration;
using MicrosoftAgentFramework.Runtime;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;
using MicrosoftAgentFramework.Services.OpenMeteo;

namespace MicrosoftAgentFramework;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<OpenAIConfig>()
            .Bind(configuration.GetSection("OpenAIConfig"))
            .ValidateDataAnnotations()
            .Validate(static options =>
                !string.IsNullOrWhiteSpace(options.ApiKey) &&
                !string.IsNullOrWhiteSpace(options.Endpoint) &&
                !string.IsNullOrWhiteSpace(options.DeploymentName),
                "OpenAIConfig requires ApiKey, Endpoint, and DeploymentName.")
            .ValidateOnStart();

        services
            .AddOptions<ThreadManagerOptions>()
            .Bind(configuration.GetSection("ThreadManager"))
            .ValidateDataAnnotations()
            .Validate(static options =>
                string.Equals(options.Provider, "json", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(options.Provider, "memory", StringComparison.OrdinalIgnoreCase),
                "ThreadManager:Provider must be either 'memory' or 'json'.")
            .ValidateOnStart();
        
        services.AddSingleton<AzureOpenAIClient>(sp =>
        {
            var openAiConfig = sp.GetRequiredService<IOptions<OpenAIConfig>>().Value;
            return new AzureOpenAIClient(
                new Uri(openAiConfig.Endpoint),
                new AzureKeyCredential(openAiConfig.ApiKey));
        });

        // Register Services
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        // Register CountriesNowApiClient (inner client for the wrapper)
        services.AddHttpClient();
        services.AddScoped<CountriesNowApiClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            return new CountriesNowApiClient(httpClient);
        });
        
        // Register CountriesNowApiClient logging wrapper
        services.AddScoped<LoggingCountriesNowApiClient>(sp =>
        {
            var innerClient = sp.GetRequiredService<CountriesNowApiClient>();
            var logger = sp.GetRequiredService<ILogger<LoggingCountriesNowApiClient>>();
            var dateTimeProvider = sp.GetRequiredService<IDateTimeProvider>();
            return new LoggingCountriesNowApiClient(innerClient, logger, dateTimeProvider);
        });
        
        // Register OpenMeteoClient with logging wrapper
        services.AddSingleton<OpenMeteo.OpenMeteoClient>(_ => new OpenMeteo.OpenMeteoClient());
        services.AddScoped<LoggingOpenMeteoClient>(sp =>
        {
            var innerClient = sp.GetRequiredService<OpenMeteo.OpenMeteoClient>();
            var logger = sp.GetRequiredService<ILogger<LoggingOpenMeteoClient>>();
            var dateTimeProvider = sp.GetRequiredService<IDateTimeProvider>();
            return new LoggingOpenMeteoClient(innerClient, logger, dateTimeProvider);
        });
        
        services.AddScoped<AgentRegistry>();
        var threadManagerProvider = configuration.GetSection("ThreadManager").GetValue<string>("Provider");
        if (string.Equals(threadManagerProvider, "json", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IThreadManager, JsonDocumentThreadManager>();
        }
        else
        {
            services.AddSingleton<IThreadManager, InMemoryThreadManager>();
        }
        services.AddScoped<IAgentRuntime, AgentRuntime>();

        // Register Agent Provider
        services.AddScoped<IAgentProvider, AzureOpenAiAgentProvider>();

        // Register Agent Implementations (keyed services)
        services.AddKeyedScoped<IAgentImplementation, AzureOpenAiChatClientImplementation>(AgentClient.ChatClient);
        services.AddKeyedScoped<IAgentImplementation, AzureOpenAiResponseClientImplementation>(AgentClient.ResponseClient);

        // Register Agent Composers (keyed services) - scoped because they depend on scoped services
        services.AddKeyedScoped<IAgentComposer, HistoricalAndCurrencyToolExpertAgentComposer>(AgentName.HistoricalAndCurrencyToolExpert);
        services.AddKeyedScoped<IAgentComposer, AgentAsToolComposer>(AgentName.AgentAsTool);
        services.AddKeyedScoped<IAgentComposer, CountryExtractorComposer>(AgentName.CountryExtractor);
        services.AddKeyedScoped<IAgentComposer, CountryDataEnricherComposer>(AgentName.CountryDataEnricher);
        services.AddKeyedScoped<IAgentComposer, OrchestratorAgentComposer>(AgentName.OrchestratorAgent);
        services.AddKeyedScoped<IAgentComposer, LocationAgentComposer>(AgentName.LocationAgent);
        services.AddKeyedScoped<IAgentComposer, WeatherAgentComposer>(AgentName.WeatherAgent);
        services.AddKeyedScoped<IAgentComposer, TranslatorAgentComposer>(AgentName.TranslatorAgent);
        services.AddKeyedScoped<IAgentComposer, ResponseTranslatorAgentComposer>(AgentName.ResponseTranslator);
        services.AddKeyedScoped<IAgentComposer, TravelIntentAgentComposer>(AgentName.TravelIntentAgent);
        services.AddKeyedScoped<IAgentComposer, BudgetPlannerAgentComposer>(AgentName.BudgetPlannerAgent);
        services.AddKeyedScoped<IAgentComposer, ItineraryPlannerAgentComposer>(AgentName.ItineraryPlannerAgent);
        services.AddKeyedScoped<IAgentComposer, LodgingAdvisorAgentComposer>(AgentName.LodgingAdvisorAgent);
        services.AddKeyedScoped<IAgentComposer, TransportAdvisorAgentComposer>(AgentName.TransportAdvisorAgent);

        return services;
    }
}
