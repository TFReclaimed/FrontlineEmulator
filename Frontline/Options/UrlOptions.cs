namespace Frontline.Options;

public class UrlOptions
{
    public const string SectionName = "UrlSettings";
    
    public string AssetBundleUrl { get; set; } = string.Empty;
    
    public string PveRulesetUrl { get; set; } = string.Empty;
    
    public string RulesetsUrl { get; set; } = string.Empty;
}