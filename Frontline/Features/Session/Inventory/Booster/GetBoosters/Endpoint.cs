using FastEndpoints;

namespace Frontline.Features.Session.Inventory.Booster.GetBoosters;

public class Endpoint : Endpoint<GetInventoryRequest, List<int>>
{
    public override void Configure()
    {
        Get("/session/booster");
    }

    public override async Task HandleAsync(GetInventoryRequest req, CancellationToken ct)
    {
        var response = new List<int> { 0 };
        
        await SendAsync(response, cancellation: ct);
    }
}