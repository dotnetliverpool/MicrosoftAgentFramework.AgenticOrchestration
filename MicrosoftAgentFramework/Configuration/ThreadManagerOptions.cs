using System.ComponentModel.DataAnnotations;

namespace MicrosoftAgentFramework.Configuration;

public class ThreadManagerOptions
{
    [Required]
    public string Provider { get; set; } = "memory";
}
