using FastEndpoints;

namespace Frontline.Features.GameConfig;

public class Endpoint : EndpointWithoutRequest<GameConfigResponse>
{
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
                    // yes, the tests really are required
                    Uri = "http://192.168.0.219/Assets/test/test"
                }
            ],
            PveRuleset = new PveRuleset
            {
                Uri = "http://192.168.0.219/pve",
                Version = 1
            },
            //MinClientVersion = "1.0"
            MinClientVersion = "1.0.15816"
        };

        await SendAsync(response);
    }
}