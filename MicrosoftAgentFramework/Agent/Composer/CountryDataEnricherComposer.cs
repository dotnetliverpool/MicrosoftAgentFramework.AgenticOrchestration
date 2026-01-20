using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services;
using MicrosoftAgentFramework.Services.CountriesNowApiClient;

namespace MicrosoftAgentFramework.Agent.Composer;

public class CountryDataEnricherComposer(IAgentProvider agentProvider, IServiceProvider serviceProvider) : IAgentComposer
{
    public AgentName Name => AgentName.CountryDataEnricher;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.Medium,
            Name = Name.ToString()
        };

        const string instructions = """
            You are an agent specialized in enriching country data with detailed information including flag colors.
            
            Extract structured data about countries including IATA codes, names, and flag colors.
            For flag colors, extract all distinct colors mentioned or that you know about the official flag.
            Be precise and accurate with country identification.
            """;

        var countriesNowApiClient = serviceProvider.GetRequiredService<CountriesNowApiClient>();
        var dateTimeProvider = serviceProvider.GetRequiredService<IDateTimeProvider>();

        List<AITool> tools = new List<AITool>()
        {
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryPopulationAsync,
                name: "get_country_population",
                description: "Gets a single country and its population data"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryCurrencyAsync,
                name: "get_country_currency",
                description: "Gets a single country and its currency"),
            
            AIFunctionFactory.Create(
                countriesNowApiClient.GetCountryPositionAsync,
                name: "get_country_position",
                description: "Gets a country's latitude and longitude coordinates"),
            
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
