using FastEndpoints;
using Frontline.Extensions;

namespace Frontline.Features.GetAssets;

public class Endpoint : EndpointWithoutRequest<AssetBundlesResponse>
{
    public override void Configure()
    {
        Get("/");
        AllowAnonymous();
        Description(x =>
        {
            x.Produces<AssetBundlesResponse>(200, "application/xml");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new AssetBundlesResponse
        {
            new() { Name = "audio" },
            new() { Name = "avatars" },
            new() { Name = "common" },
            new() { Name = "countryflags" },
            new() { Name = "enteringmatchui" },
            new() { Name = "eventanim1" },
            new() { Name = "fue" },
            new() { Name = "gameboard1" },
            new() { Name = "gameui" },
            new() { Name = "guildinsignia" },
            new() { Name = "hangarui" },
            new() { Name = "localization.english" },
            new() { Name = "mainui" },
            new() { Name = "missionsui" },
            new() { Name = "portraitbase" },
            new() { Name = "pveui" },
            new() { Name = "scenes" },
            new() { Name = "shadowbox_etc" },
            new() { Name = "shadowbox_female" },
            new() { Name = "shadowbox_heavy" },
            new() { Name = "shadowbox_medium" },
            new() { Name = "shadowbox_titan" },
            new() { Name = "tokens" }
        };
        
        await this.SendXmlAsync(response);
    }
}