using FastEndpoints;

namespace Frontline.Features.Session.Inventory.Booster.OpenBooster;

public class Endpoint : Endpoint<OpenBoosterPackRequest, BoosterPackResponse>
{
    public override void Configure()
    {
        Post("/session/booster/{BoosterId}");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(OpenBoosterPackRequest req, CancellationToken ct)
    {
        var response = new BoosterPackResponse
        {
            Cards = [],
            Resources = []
        };
        
        await SendAsync(response, cancellation: ct);
    }
}