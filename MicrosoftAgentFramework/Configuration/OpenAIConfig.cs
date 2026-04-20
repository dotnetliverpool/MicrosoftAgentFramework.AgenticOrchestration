namespace MicrosoftAgentFramework.Configuration;

using System.ComponentModel.DataAnnotations;

public class OpenAIConfig
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    [Url]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string DeploymentName { get; set; } = "gpt-4";
}