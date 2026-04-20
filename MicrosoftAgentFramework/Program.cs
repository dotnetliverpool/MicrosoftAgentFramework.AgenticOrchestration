using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
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
    .WithDescription("Runs intent, budget, itinerary, lodging, and transport agents with concern-scoped memory and explicit structured handoffs.")
    .Produces<TravelPlannerResponse>()
    .Produces<TravelPlannerFollowUpResponse>()
    .Produces<string>(StatusCodes.Status400BadRequest)
    .WithOpenApi();


app.MapGet("/agentWithTool", async (AgentRegistry registry, string message, CancellationToken cancellationToken) =>
{
    var agent = registry.Get(AgentName.HistoricalAndCurrencyToolExpert);
    AgentResponse response = await agent.RunAsync(message: message, cancellationToken: cancellationToken);
    
    return Results.Ok(response.Text);
})
.WithName("AgentWithTools")
.WithOpenApi();

app.MapGet("agentAsTool", async (AgentRegistry registry, string message, CancellationToken cancellationToken) =>
    {
        var agent = registry.Get(AgentName.AgentAsTool);
        var response = await agent.RunAsync(message: message, cancellationToken: cancellationToken);
        
        return Results.Ok(response.Text );
    })
    .WithName("AgentAsTool")
    .WithOpenApi();

app.MapGet("reflectingExecutor", async (
        AgentRegistry registry, 
        string message, 
        string responseLanguage = "English", 
        CancellationToken cancellationToken = default) =>
    {
        // Create executors
        var countryExtractor = new CountryExtractorExecutor(registry);
        var cacheChecker = new CountryCacheCheckerExecutor();
        var dataEnricher = new CountryDataEnricherExecutor(registry);
        var responseFormatter = new ResponseFormatterExecutor(registry, responseLanguage);

        // Build workflow
        WorkflowBuilder builder = new(countryExtractor);

        // Add switch after extraction: check if Success == false
        builder.AddSwitch(
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
        builder.AddSwitch(
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
        builder.AddEdge(
            source: dataEnricher,
            target: responseFormatter);

        // Execute workflow
        var workflow = builder.Build();
        StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow: workflow, input: message, cancellationToken: cancellationToken);

        string? finalResult = null;
        
        await foreach (WorkflowEvent evt in run.WatchStreamAsync(cancellationToken))
        {
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
    .WithOpenApi();

app.MapGet("agentOrchestrationHandoff", async (
    AgentRegistry registry, 
    string message, 
    ILoggerFactory loggerFactory,
    string responseLanguage = "English", 
    CancellationToken cancellationToken = default) =>
{
    var logger = loggerFactory.CreateLogger("AgentOrchestrationHandoff");
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
    string? lastExecutorId = null;

    await foreach (WorkflowEvent evt in run.WatchStreamAsync(cancellationToken))
    {
        switch (evt)
        {
            case WorkflowOutputEvent output:
                // Get final output
                List<ChatMessage> data = (List<ChatMessage>)output.Data;
                finalResponse += data[data.Count - 1].Text;
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
.WithOpenApi();

await app.RunAsync();