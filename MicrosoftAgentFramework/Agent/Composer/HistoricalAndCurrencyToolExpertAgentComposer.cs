using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;

namespace MicrosoftAgentFramework.Agent.Composer;

public class HistoricalAndCurrencyToolExpertAgentComposer(IAgentProvider agentProvider, IServiceProvider serviceProvider) : IAgentComposer
{
    public AgentName Name => AgentName.HistoricalAndCurrencyToolExpert;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.High,
            Name = Name.ToString()
        };

        const string instructions = """
            you are an expert that can use tools to answer questions about country names
            you do not respond if the user message does not contain a country name
            """;

        var countriesNowApiClient = serviceProvider.GetRequiredService<LoggingCountriesNowApiClient>();
        var dateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();

        List<AITool> tools = new List<AITool>()
        {
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCityPopulationAsync,
                name: "get_city_population",
                description: "Gets historical population data for a city"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryPopulationAsync,
                name: "get_country_population",
                description: "Gets historical population data for a country"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryCurrencyAsync,
                name: "get_country_currency",
                description: "Gets currency information for a country"),
            
            AIFunctionFactory.Create(
                () => Task.FromResult(dateTimeProvider.UtcNow),
                name: "get_current_utc_time",
                description: "Gets the current UTC date and time")
        };

        return agentProvider.GetAgent(
            aiModel: aiModel,
            instructions: instructions,
            tools: tools,
            contextProviderFactory: null);
    }
}
