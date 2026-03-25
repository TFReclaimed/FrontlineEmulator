namespace Frontline.Endpoints.GameConfig;

public class GameConfigRequest
{
    public required GameConfigParams Param { get; set; }
}

public class GameConfigParams
{
    public Platform Platform { get; set; }
}

public class GameConfigResponse
{
    public required List<AssetBundleInfo> AssetBundleInfo { get; set; }
    public required PveRuleset PveRuleset { get; set; }
    public string MinClientVersion { get; set; } = string.Empty;
}

public class AssetBundleInfo
{
    public string Uri { get; set; } = string.Empty;
}

public class PveRuleset
{
    public string Uri { get; set; } = string.Empty;
    public int Version { get; set; }
}