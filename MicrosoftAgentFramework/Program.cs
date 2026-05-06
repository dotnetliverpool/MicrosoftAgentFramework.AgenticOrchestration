using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using MicrosoftAgentFramework;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Agent.Executors;
using MicrosoftAgentFramework.Models;
using MicrosoftAgentFramework.Runtime;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/travelPlanner", async (
        IAgentRuntime runtime,
        string message,
        string sessionId = "default-session",
        CancellationToken cancellationToken = default) =>
{
    var intentSessionId = $"{sessionId}:{AgentName.TravelIntentAgent}";
    var budgetSessionId = $"{sessionId}:{AgentName.BudgetPlannerAgent}";
    var itinerarySessionId = $"{sessionId}:{AgentName.ItineraryPlannerAgent}";
    var lodgingSessionId = $"{sessionId}:{AgentName.LodgingAdvisorAgent}";
    var transportSessionId = $"{sessionId}:{AgentName.TransportAdvisorAgent}";

    AgentRunResult<TravelIntentResponse> intentRun = await runtime
        .WithAgent(AgentName.TravelIntentAgent)
        .ForSession(intentSessionId)
        .RunAsync<TravelIntentResponse>(message, cancellationToken);

    var intent = intentRun.Result;
    if (intent is null)
        return Results.BadRequest("Unable to extract travel intent.");
    if (!intent.Success)
    {
        return Results.Ok(new TravelPlannerFollowUpResponse
        {
            Status = "needs_more_info",
            Intent = intent,
            Guidance = intent.UserPromptMessage ?? "Please provide destination and basic trip details."
        });
    }

    var handoffContext = JsonSerializer.Serialize(intent);

    AgentRunResult<TravelBudgetPlan> budgetRun = await runtime
        .WithAgent(AgentName.BudgetPlannerAgent)
        .ForSession(budgetSessionId)
        .RunAsync<TravelBudgetPlan>($"Travel intent context: {handoffContext}", cancellationToken);

    AgentRunResult<TravelItineraryPlan> itineraryRun = await runtime
        .WithAgent(AgentName.ItineraryPlannerAgent)
        .ForSession(itinerarySessionId)
        .RunAsync<TravelItineraryPlan>($"Travel intent context: {handoffContext}. Budget guidance: {JsonSerializer.Serialize(budgetRun.Result)}", cancellationToken);

    AgentRunResult<TravelLodgingPlan> lodgingRun = await runtime
        .WithAgent(AgentName.LodgingAdvisorAgent)
        .ForSession(lodgingSessionId)
        .RunAsync<TravelLodgingPlan>($"Travel intent context: {handoffContext}. Budget guidance: {JsonSerializer.Serialize(budgetRun.Result)}. Itinerary summary: {JsonSerializer.Serialize(itineraryRun.Result)}", cancellationToken);

    AgentRunResult<TravelTransportPlan> transportRun = await runtime
        .WithAgent(AgentName.TransportAdvisorAgent)
        .ForSession(transportSessionId)
        .RunAsync<TravelTransportPlan>($"Travel intent context: {handoffContext}. Itinerary summary: {JsonSerializer.Serialize(itineraryRun.Result)}", cancellationToken);

    if (budgetRun.Result is null || itineraryRun.Result is null || lodgingRun.Result is null || transportRun.Result is null)
        return Results.BadRequest("Travel planning could not be completed from the current request details.");

    var response = new TravelPlannerResponse
    {
        Intent = intent,
        Budget = budgetRun.Result,
        Itinerary = itineraryRun.Result,
        Lodging = lodgingRun.Result,
        Transport = transportRun.Result
    };

    return Results.Ok(response);
})
    .WithName("TravelPlanner")
    .WithSummary("Plan travel with isolated specialist agents")
    .WithDescription("Runs intent, budget, itinerary, lodging, and transport agents with concern-scoped memory and explicit structured handoffs. Try message: 'Plan a 5-day trip to Lisbon for two travelers with a medium budget and foodie interests.'")
    .Produces<TravelPlannerResponse>()
    .Produces<TravelPlannerFollowUpResponse>()
    .Produces<string>(StatusCodes.Status400BadRequest)
    .WithOpenApi();


app.MapGet("/agentWithTool", async (IAgentRuntime runtime, string message, CancellationToken cancellationToken) =>
{
    AgentRunResult result = await runtime
        .WithAgent(AgentName.CountryInfoToolAgent)
        .ForSession("default-session")
        .RunAsync(message, cancellationToken);
    
    return Results.Ok(result.Text);
})
.WithName("AgentWithTools")
.WithSummary("Single agent with all tools - observe full schema cost per call")
.WithDescription("One agent with 8 tools. Every tool schema is serialized into the prompt on every LLM call, even when only one tool is needed. Use the same message on /agentAsTool and compare the [DEMO] token blocks in the console. Try message: 'What is the currency of Japan?' or 'What are the most populous cities in Germany?'")
.WithOpenApi();

app.MapGet("agentAsTool", async (IAgentRuntime runtime, string message, CancellationToken cancellationToken) =>
    {
        AgentRunResult result = await runtime
            .WithAgent(AgentName.AgentAsTool)
            .ForSession("default-session")
            .RunAsync(message, cancellationToken);
    
        return Results.Ok(result.Text);
    })
    .WithName("AgentAsTool")
    .WithSummary("Orchestrator routing to specialist sub-agents - lazy schema loading")
    .WithDescription("An orchestrator carries only thin sub-agent wrapper schemas. Only the selected sub-agent loads its own tools. For a focused question, prompt and completion tokens will be lower than /agentWithTool. Try message: 'What is the currency of Japan?' (routes to currency sub-agent only) or 'What is the weather in Tokyo and the currency of Japan?' (routes to two sub-agents).")
    .WithOpenApi();

app.MapGet("reflectingExecutor", async (
        IAgentRuntime runtime, 
        ILoggerFactory loggerFactory,
        string message, 
        string responseLanguage = "English", 
        CancellationToken cancellationToken = default) =>
    {
        var logger = loggerFactory.CreateLogger("ReflectingExecutor");

        // Create executors
        var countryExtractor = new CountryExtractorExecutor(runtime);
        var cacheChecker = new CountryCacheCheckerExecutor();
        var dataEnricher = new CountryDataEnricherExecutor(runtime);
        var responseFormatter = new ResponseFormatterExecutor(runtime, responseLanguage);
        // Build workflow
        WorkflowBuilder workflowBuilder = new(countryExtractor);

        // Add switch after extraction: check if Success == false
        workflowBuilder.AddSwitch(
            source: countryExtractor,
            switchBuilder =>
            {
                switchBuilder.AddCase<ExtractCountryNameResponse>(
                    x => !x!.Success,
                    [responseFormatter]
                );
                switchBuilder.AddCase<ExtractCountryNameResponse>(
                    x => x!.Success, 
                    [cacheChecker]);
            });
        // Add switch after cache check: if found return formatted, else enrich
        workflowBuilder.AddSwitch(
            source: cacheChecker,
            switchBuilder =>
            {
                switchBuilder.AddCase<Country>(
                    x => x != null, 
                    [responseFormatter]);
                switchBuilder.AddCase<ExtractCountryNameResponse>(
                    x => x != null, 
                    [dataEnricher]);
            });

        // Add edge from data enricher to formatter
        workflowBuilder.AddEdge(
            source: dataEnricher,
            target: responseFormatter);

        // Execute workflow
        var workflow = workflowBuilder.Build();
        StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow: workflow, input: message, cancellationToken: cancellationToken);

        string? finalResult = null;
        
        await foreach (WorkflowEvent evt in run.WatchStreamAsync(cancellationToken))
        {
            WorkflowLogger.Log(evt, logger);
            if (evt is not ExecutorCompletedEvent executorComplete) continue;
            if (executorComplete.ExecutorId == "ResponseFormatter")
            {
                finalResult = executorComplete.Data?.ToString();
            }
        }
        
        return finalResult != null ?
            Results.Ok(finalResult) : 
            Results.BadRequest("Workflow execution did not complete successfully");
    })
    .WithName("ReflectingExecutorBuilder")
    .WithSummary("Run a reflecting executor workflow with cache fallback")
    .WithDescription("Builds and runs a workflow with extraction, cache check, enrichment, and narration. Try message: 'Tell me about France' or 'What are Germany's flag colors?'")
    .WithOpenApi();

app.MapGet("agentOrchestrationHandoff", async (
    AgentRegistry registry, 
    string message, 
    ILoggerFactory loggerFactory,
    string responseLanguage = "English", 
    CancellationToken cancellationToken = default) =>
{
    var logger = loggerFactory.CreateLogger("AgentHandoff");

    // Get agents
    var orchestratorAgent = registry.Get(AgentName.OrchestratorAgent).AsBuilder().Build();
    var locationAgent = registry.Get(AgentName.LocationAgent).AsBuilder().Build();
    var weatherAgent = registry.Get(AgentName.WeatherAgent).AsBuilder().Build();
    var translatorAgent = registry.Get(AgentName.TranslatorAgent, new Dictionary<string, object> 
    { 
        { "responseLanguage", responseLanguage } 
    }).AsBuilder().Build();

    // Build handoff workflow
#pragma warning disable MAAIW001
    Workflow workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(orchestratorAgent)
#pragma warning restore MAAIW001
        .WithHandoffs(orchestratorAgent, [locationAgent, weatherAgent])
        .WithHandoffs([locationAgent, weatherAgent], orchestratorAgent)
        .WithHandoffs(orchestratorAgent, [translatorAgent])
        .Build();
    
    // Execute workflow
    StreamingRun run = await InProcessExecution
        .RunStreamingAsync(workflow: workflow, input: message, cancellationToken: cancellationToken);
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

    string? finalResponse = null;

    await foreach (WorkflowEvent evt in run.WatchStreamAsync(cancellationToken))
    {
        WorkflowLogger.Log(evt, logger);
        switch (evt)
        {
            case WorkflowOutputEvent output:
                // Workflow output payload can vary by emitter. Handle known shapes safely.
                if (output.Data is List<ChatMessage> messages && messages.Count > 0)
                {
                    finalResponse += messages[^1].Text;
                }
                else if (output.Data is ChatMessage chatMessage)
                {
                    finalResponse += chatMessage.Text;
                }
                else if (output.Data is AgentResponse agentResponse)
                {
                    finalResponse += agentResponse.Text;
                }
                else if (output.Data is not null)
                {
                    finalResponse += output.Data.ToString();
                }
                break;

            case ExecutorFailedEvent failedEvent:
                if (failedEvent.Data is Exception ex)
                {
                    return Results.BadRequest($"Error in workflow: {ex.Message}");
                }
                break;
        }
    }

    return finalResponse != null
        ? Results.Ok( finalResponse)
        : Results.BadRequest("Workflow execution did not complete successfully");
})
.WithName("AgentOrchestrationHandoff")
.WithSummary("Run multi-agent handoff orchestration")
.WithDescription("Coordinates orchestrator, location, weather, and translator agents in a handoff graph. Try message: 'What are the major cities in Germany and the current weather there?' and set responseLanguage=Spanish.")
.WithOpenApi();

await app.RunAsync();