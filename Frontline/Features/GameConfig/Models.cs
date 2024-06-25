namespace Frontline.Features.GameConfig;

public class GameConfigResponse
{
    public List<AssetBundleInfo> AssetBundleInfo { get; set; }
    public PveRuleset PveRuleset { get; set; }
    public string MinClientVersion { get; set; }
}

public class AssetBundleInfo
{
    public string Uri { get; set; }
}

public class PveRuleset
{
    public string Uri { get; set; }
    public int Version { get; set; }
}