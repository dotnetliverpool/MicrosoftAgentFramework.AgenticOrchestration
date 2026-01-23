namespace MicrosoftAgentFramework.Models;

public class ExtractCountryNameResponse
{
    public string ISOCode { get; set; }
    public string CountryName { get; set; }
    public bool Success { get; set; }
    public string? UserPromptMessage { get; set; }
}
