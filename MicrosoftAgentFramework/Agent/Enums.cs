namespace MicrosoftAgentFramework.Agent;

public enum AgentName
{
    StructuredOutput,
    AgentWithTools,
    AgentAsTool,
    CountryExtractor,
    CountryDataEnricher,
    OrchestratorAgent,
    LocationAgent,
    WeatherAgent,
    TranslatorAgent,
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

public enum SkylarIntent {HotelSearch, FlightSearch, RestaurantSearch, GeneralTravel}