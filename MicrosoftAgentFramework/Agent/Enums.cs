using System.Diagnostics.CodeAnalysis;

namespace MicrosoftAgentFramework.Agent;

public enum AgentName
{
    HistoricalAndCurrencyToolExpert,
    AgentAsTool,
    CountryExtractor,
    CountryDataEnricher,
    OrchestratorAgent,
    LocationAgent,
    WeatherAgent,
    TranslatorAgent,
    ResponseTranslator,
    TravelIntentAgent,
    BudgetPlannerAgent,
    ItineraryPlannerAgent,
    LodgingAdvisorAgent,
    TransportAdvisorAgent
}

public enum AgentReasoningEffortLevel
{
    Minimal, Low, Medium, High
}

public enum AiProvider
{
    AzureOpenAi
}

public enum AgentClient
{
    ChatClient, ResponseClient
}