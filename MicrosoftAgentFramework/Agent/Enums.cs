using System.Diagnostics.CodeAnalysis;

namespace MicrosoftAgentFramework.Agent;

public enum AgentName
{
    CountryISOExpert,
    HistoricalAndCurrencyToolExpert,
    AgentAsTool,
    CountryExtractor,
    CountryDataEnricher,
    CountryFlagExpert,
    OrchestratorAgent,
    LocationAgent,
    WeatherAgent,
    TranslatorAgent,
    ResponseTranslator,
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