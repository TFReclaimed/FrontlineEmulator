using System.ComponentModel.DataAnnotations;

namespace Frontline.Options;

public class ChatOptions
{
    public const string SectionName = "ChatSettings";
    
    [Range(1, 65535)]
    public int Port { get; set; } = 5222;
    
    [MaxLength(140)]
    public string WelcomeMessage { get; set; } = string.Empty;
    
    public bool EnableXmlLogging { get; set; }
}