using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Agent.Composer;
using MicrosoftAgentFramework.Configuration;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;
using MicrosoftAgentFramework.Services.OpenMeteo;

namespace MicrosoftAgentFramework;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Configuration
        services.Configure<OpenAIConfig>(configuration.GetSection("OpenAIConfig"));

        // Register Azure OpenAI Client
        var openAiConfig = configuration.GetSection("OpenAIConfig").Get<OpenAIConfig>() 
            ?? throw new InvalidOperationException("OpenAI configuration is missing");
        
        services.AddSingleton<AzureOpenAIClient>(sp =>
        {
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

        // Register Agent Provider
        services.AddScoped<IAgentProvider, AzureOpenAiAgentProvider>();

        // Register Agent Implementations (keyed services)
        services.AddKeyedScoped<IAgentImplementation, AzureOpenAiChatClientImplementation>(AgentClient.ChatClient);
        services.AddKeyedScoped<IAgentImplementation, AzureOpenAiResponseClientImplementation>(AgentClient.ResponseClient);

        // Register Agent Composers (keyed services) - scoped because they depend on scoped services
        services.AddKeyedScoped<IAgentComposer, CountryISOAgentComposer>(AgentName.CountryISOExpert);
        services.AddKeyedScoped<IAgentComposer, HistoricalAndCurrencyToolExpertAgentComposer>(AgentName.HistoricalAndCurrencyToolExpert);
        services.AddKeyedScoped<IAgentComposer, AgentAsToolComposer>(AgentName.AgentAsTool);
        services.AddKeyedScoped<IAgentComposer, CountryExtractorComposer>(AgentName.CountryExtractor);
        services.AddKeyedScoped<IAgentComposer, CountryDataEnricherComposer>(AgentName.CountryDataEnricher);
        services.AddKeyedScoped<IAgentComposer, CountryFlagExpertComposer>(AgentName.CountryFlagExpert);
        services.AddKeyedScoped<IAgentComposer, OrchestratorAgentComposer>(AgentName.OrchestratorAgent);
        services.AddKeyedScoped<IAgentComposer, LocationAgentComposer>(AgentName.LocationAgent);
        services.AddKeyedScoped<IAgentComposer, WeatherAgentComposer>(AgentName.WeatherAgent);
        services.AddKeyedScoped<IAgentComposer, TranslatorAgentComposer>(AgentName.TranslatorAgent);
        services.AddKeyedScoped<IAgentComposer, ResponseTranslatorAgentComposer>(AgentName.ResponseTranslator);

        return services;
    }
}
