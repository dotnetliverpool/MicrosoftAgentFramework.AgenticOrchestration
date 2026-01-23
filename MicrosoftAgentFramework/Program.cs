using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Agent.Executors;
using MicrosoftAgentFramework.Models;
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

app.MapGet("/structuredOutput", async (AgentRegistry registry, string message, CancellationToken cancellationToken) =>
    {
        
        ChatClientAgent agent = registry.Get(AgentName.CountryISOExpert);
        
        ChatClientAgentRunResponse<ExtractCountryNameResponse> countryNameResponseResult = await agent.
            RunAsync<ExtractCountryNameResponse>(message: message, cancellationToken: cancellationToken);
        
        ExtractCountryNameResponse extractCountryNameResponse = countryNameResponseResult.Result;
        if (extractCountryNameResponse.Success == false)
        {
            return Results.BadRequest(extractCountryNameResponse.UserPromptMessage);
        }

        List<Country> countriesData = new List<Country>();
        
        var existingCountry = countriesData.FirstOrDefault(x => 
            string.Equals(x.ISOCode, extractCountryNameResponse.ISOCode, StringComparison.OrdinalIgnoreCase));

        if (existingCountry is not null)
        {
            return Results.Ok(existingCountry);
        }
        
        ChatClientAgent flagExpertAgent = registry.Get(AgentName.CountryFlagExpert);
        
        ChatClientAgentRunResponse<Country> countryDataResponseResult = await flagExpertAgent.
            RunAsync<Country>(message: $"what are the colors in the official flag of {extractCountryNameResponse.CountryName}", 
                cancellationToken: cancellationToken);
        
        Country countryData = countryDataResponseResult.Result;
        countryData.CountryName ??= extractCountryNameResponse.CountryName;
        
        countriesData.Add(countryData);
        return Results.Ok(countryData);

    })
    .WithName("StructuredOutput")
    .WithSummary("Extract country information and flag colors from user messages")
    .WithDescription("Uses AI agents to extract country name, ISO 3166-1 alpha-3 code, and flag colors from natural language messages containing geographic references.")
    .Produces<Country>()
    .Produces<string>(StatusCodes.Status400BadRequest)
    .WithOpenApi();


app.MapGet("/agentWithTool", async (AgentRegistry registry, string message, CancellationToken cancellationToken) =>
{
    var agent = registry.Get(AgentName.HistoricalAndCurrencyToolExpert);
    var response = await agent.RunAsync(message: message, cancellationToken: cancellationToken);
    
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
        StreamingRun run = await InProcessExecution.StreamAsync(workflow: workflow, input: message, cancellationToken: cancellationToken);

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
    Workflow workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(orchestratorAgent)
        .WithHandoffs(orchestratorAgent, [locationAgent, weatherAgent])
        .WithHandoffs([locationAgent, weatherAgent], orchestratorAgent)
        .WithHandoffs(orchestratorAgent, [translatorAgent])
        .Build();

    // Execute workflow
    StreamingRun run = await InProcessExecution
        .StreamAsync(workflow:workflow, input:message, cancellationToken:cancellationToken);
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

    string? finalResponse = null;
    string? lastExecutorId = null;

    await foreach (WorkflowEvent evt in run.WatchStreamAsync(cancellationToken))
    {
        switch (evt)
        {
            case AgentRunUpdateEvent updateEvent:
                if (updateEvent.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is { } call)
                {
                    logger.LogInformation($"Call '{call.Name}' with arguments: {JsonSerializer.Serialize(call.Arguments)}]");
                }
                break;
            
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