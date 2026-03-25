using FastEndpoints;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Endpoints.GameConfig;

public class GetGameConfigEndpoint : Endpoint<GameConfigRequest, GameConfigResponse>
{
    private readonly IOptions<UrlOptions> _urlOptions;

    public GetGameConfigEndpoint(IOptions<UrlOptions> urlOptions)
    {
        _urlOptions = urlOptions;
    }

    public override void Configure()
    {
        Get("/init");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GameConfigRequest req, CancellationToken ct)
    {
        var assetBundleUrl = string.Format(_urlOptions.Value.AssetBundleUrl, req.Param.Platform.ToString());
        if (!assetBundleUrl.Contains("cdn"))
        {
            // url has to go two levels deep
            assetBundleUrl = assetBundleUrl.TrimEnd('/') + "/test/test/";
        }

        var response = new GameConfigResponse
        {
            AssetBundleInfo = [
                new AssetBundleInfo
                {
                    Uri = assetBundleUrl
                }
            ],
            PveRuleset = new PveRuleset
            {
                Uri = _urlOptions.Value.PveRulesetUrl,
                Version = 1
            },
            //MinClientVersion = "1.0"
            MinClientVersion = "1.0.15816"
        };

        await Send.OkAsync(response);
    }
}