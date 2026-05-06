using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Agent.Tools;

namespace MicrosoftAgentFramework.Agent.Composer;

public class CountryInfoToolAgentComposer(
    IAgentProvider agentProvider,
    CountryTools countryTools,
    WeatherTools weatherTools,
    DateTimeTools dateTimeTools) : IAgentComposer
{
    public AgentName Name => AgentName.CountryInfoToolAgent;

    public ChatClientAgent Get(Dictionary<string, object>? context = null)
    {
        var aiModel = new AiModel
        {
            ClientType = AgentClient.ChatClient,
            ReasoningEffortLevel = AgentReasoningEffortLevel.High,
            Name = Name.ToString()
        };

        const string instructions = """
            You are an expert that uses tools to answer questions about country population and currency.
            If no country name is present in the message, respond with a brief message asking the user to specify a country.
            """;

        List<AITool> tools = new()
        {
            countryTools.CityPopulation,
            countryTools.CountryPopulation,
            countryTools.CountryCurrency,
            countryTools.CountryPosition,
            countryTools.FilterCitiesPopulation,
            weatherTools.WeatherByLocation,
            weatherTools.WeatherByCoordinates,
            dateTimeTools.CurrentUtcTime,
            countryTools.CityPopulation,
            countryTools.CountryPopulation,
            countryTools.CountryCurrency,
            countryTools.CountryPosition,
            countryTools.FilterCitiesPopulation,
            weatherTools.WeatherByLocation,
            weatherTools.WeatherByCoordinates
        };

        return agentProvider.GetAgent(aiModel, instructions, tools);
    }
}
