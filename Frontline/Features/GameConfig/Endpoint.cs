using FastEndpoints;
using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Features.GameConfig;

public class Endpoint : EndpointWithoutRequest<GameConfigResponse>
{
    private readonly IOptions<UrlOptions> _urlOptions;

    public Endpoint(IOptions<UrlOptions> urlOptions)
    {
        _urlOptions = urlOptions;
    }

    public override void Configure()
    {
        Get("/init");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new GameConfigResponse
        {
            AssetBundleInfo = [
                new AssetBundleInfo
                {
                    // url has to go two levels deep
                    Uri = _urlOptions.Value.AssetBundleUrl
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