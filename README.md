# MicrosoftAgentFramework

An ASP.NET Core sample project that demonstrates multi-agent orchestration patterns with Microsoft Agents AI, including:

- Specialist-agent travel planning with structured handoffs.
- Session/thread-backed runtime orchestration.
- Token usage telemetry per agent run.
- Configurable in-memory or JSON-backed thread/session storage.

## Project layout

- `MicrosoftAgentFramework/Program.cs` - Minimal API routes and workflow demos.
- `MicrosoftAgentFramework/DependencyInjection.cs` - Service registration and options binding.
- `MicrosoftAgentFramework/Agent` - Agent provider, registry, and composers.
- `MicrosoftAgentFramework/Runtime` - Runtime abstraction, session managers, and run metadata.
- `MicrosoftAgentFramework/Models` - Structured response contracts.

## Prerequisites

- .NET 8 SDK
- Azure OpenAI deployment

## Configuration

Configure `MicrosoftAgentFramework/appsettings.json`:

```json
{
  "OpenAIConfig": {
    "ApiKey": "<your-api-key>",
    "Endpoint": "https://<your-resource>.openai.azure.com",
    "DeploymentName": "<your-deployment>"
  },
  "ThreadManager": {
    "Provider": "memory"
  }
}
```

`ThreadManager:Provider` values:

- `memory` (default): in-process session state.
- `json`: persisted JSON sessions under `App_Data/sessions`.

Options are validated at startup.

## Run

From repository root:

```bash
dotnet run --project MicrosoftAgentFramework/MicrosoftAgentFramework.csproj
```

Swagger UI is available in development.

## Active API routes

- `GET /travelPlanner`
- `GET /agentWithTool`
- `GET /agentAsTool`
- `GET /reflectingExecutor`
- `GET /agentOrchestrationHandoff`

## Development workflow

```bash
dotnet restore
dotnet build
```

Keep changes focused and verify builds locally before opening a PR.

## License

This project is licensed under the MIT License. See `LICENSE`.
