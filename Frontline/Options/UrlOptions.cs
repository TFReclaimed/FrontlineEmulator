namespace Frontline.Options;

[OptionsSection("UrlSettings")]
public class UrlOptions
{
    public string AssetBundleUrl { get; set; } = string.Empty;
    
    public string PveRulesetUrl { get; set; } = string.Empty;
    
    public string RulesetsUrl { get; set; } = string.Empty;
    
    public string ToyUrl { get; set; } = string.Empty;
}