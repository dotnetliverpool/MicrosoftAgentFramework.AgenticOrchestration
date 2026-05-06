using Microsoft.Extensions.AI;
using MicrosoftAgentFramework.Services.OpenMeteo;

namespace MicrosoftAgentFramework.Agent.Tools;

public class WeatherTools(LoggingOpenMeteoClient client)
{
    public AITool WeatherByLocation { get; } = AIFunctionFactory.Create(
        (string location, CancellationToken cancellationToken) => client.QueryAsync(location, cancellationToken),
        name: "get_weather_by_location",
        description: "Gets weather forecast for a location by name.");

    public AITool WeatherByCoordinates { get; } = AIFunctionFactory.Create(
        (float latitude, float longitude, CancellationToken cancellationToken) =>
            client.QueryAsync(latitude, longitude, cancellationToken),
        name: "get_weather_by_coordinates",
        description: "Gets weather forecast for a location by latitude and longitude coordinates.");
}
