using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Agent;
using MicrosoftAgentFramework.Agent.Executors;
using MicrosoftAgentFramework.Models;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
        
        ChatClientAgent agent = registry.Get(AgentName.StructuredOutput);
        
        ChatClientAgentRunResponse<ExtractCountryNameResponse> countryNameResponseResult = await agent.
            RunAsync<ExtractCountryNameResponse>(message: message, cancellationToken: cancellationToken);
        
        ExtractCountryNameResponse extractCountryNameResponse = countryNameResponseResult.Result;
        if (extractCountryNameResponse.Success == false)
        {
            return Results.BadRequest(extractCountryNameResponse.UserPromptMessage);
        }

        List<Country> countriesData = new List<Country>();
        
        var existingCountry = countriesData.FirstOrDefault(x => 
            string.Equals(x.IATACode, extractCountryNameResponse.IATACode, StringComparison.OrdinalIgnoreCase));

        if (existingCountry is not null)
        {
            return Results.Ok(existingCountry);
        }
        
        ChatClientAgentRunResponse<Country> countryDataResponseResult = await agent.
            RunAsync<Country>(message: $"how many colors are there in the official flag of {extractCountryNameResponse.IATACode}", 
                cancellationToken: cancellationToken);
        
        Country countryData = countryDataResponseResult.Result;
        
        countriesData.Add(countryData);
        return Results.Ok(countryData);

    })
    .WithName("StructuredOutput")
    .WithOpenApi();


app.MapGet("/agentWithTool", async (AgentRegistry registry, string message, CancellationToken cancellationToken) =>
{
    ChatClientAgent agent = registry.Get(AgentName.AgentWithTools);
    
    var response = await agent.RunAsync(message: message, cancellationToken: cancellationToken);
    
    return Results.Ok(new { response = response.Text });
})
.WithName("AgentWithTools")
.WithOpenApi();

app.MapGet("agentAsTool", async (AgentRegistry registry, string message, CancellationToken cancellationToken) =>
    {
        ChatClientAgent agent = registry.Get(AgentName.AgentAsTool);
        
        var response = await agent.RunAsync(message: message, cancellationToken: cancellationToken);
        
        return Results.Ok(new { response = response.Text });
    })
    .WithName("AgentAsTool")
    .WithOpenApi();

app.MapGet("reflectingExecutor", async (AgentRegistry registry, string message, 
        string responseLanguage = "English", CancellationToken cancellationToken = default) =>
    {
        // Create executors
        var countryExtractor = new CountryExtractorExecutor(registry);
        var cacheChecker = new CountryCacheCheckerExecutor();
        var dataEnricher = new CountryDataEnricherExecutor(registry);
        var responseFormatter = new ResponseFormatterExecutor(responseLanguage);
        var errorResponse = new ErrorResponseExecutor(responseLanguage);

        // Build workflow
        WorkflowBuilder builder = new(countryExtractor);

        // Add switch after extraction: check if Success == false
        builder.AddSwitch(
            source: countryExtractor,
            switchBuilder =>
            {
                switchBuilder.AddCase<ExtractCountryNameResponse>(
                    x => !x!.Success,
                    [errorResponse]
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
            if (executorComplete.ExecutorId is "ResponseFormatter" or "ErrorResponse")
            {
                finalResult = executorComplete.Data?.ToString();
            }
        }
        
        return finalResult != null ?
            Results.Content(finalResult, "application/json") : 
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
    var orchestratorAgent = registry.Get(AgentName.OrchestratorAgent);
    var locationAgent = registry.Get(AgentName.LocationAgent);
    var weatherAgent = registry.Get(AgentName.WeatherAgent);
    var translatorAgent = registry.Get(AgentName.TranslatorAgent, new Dictionary<string, object> 
    { 
        { "responseLanguage", responseLanguage } 
    });

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
                // Accumulate agent responses
                if (!string.Equals(updateEvent.ExecutorId, lastExecutorId, StringComparison.Ordinal))
                {
                    lastExecutorId = updateEvent.ExecutorId;
                    logger.LogInformation($"Executor update received from : {updateEvent.ExecutorId ?? updateEvent.Update.AuthorName}");
                }
                logger.LogInformation(updateEvent.Update.Text);
                
                if (updateEvent.Update.Contents.OfType<FunctionCallContent>().FirstOrDefault() is { } call)
                {
                    logger.LogInformation($"Call '{call.Name}' with arguments: {JsonSerializer.Serialize(call.Arguments)}]");
                }
                break;
            
            case WorkflowOutputEvent output:
                // Get final output
                finalResponse += output.As<string>();
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
        ? Results.Ok(new { response = finalResponse })
        : Results.BadRequest("Workflow execution did not complete successfully");
})
.WithName("AgentOrchestrationHandoff")
.WithOpenApi();

