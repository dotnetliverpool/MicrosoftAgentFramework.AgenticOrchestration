using System.Diagnostics.CodeAnalysis;

namespace MicrosoftAgentFramework.Agent;

public enum AgentName
{
    CountryInfoToolAgent,
    AgentAsTool,
    CountryExtractor,
    CountryDataEnricher,
    OrchestratorAgent,
    LocationAgent,
    WeatherAgent,
    TranslatorAgent,
    CountryDataNarratorAgent,
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

public enum AgentClient
{
    ChatClient, ResponseClient
}