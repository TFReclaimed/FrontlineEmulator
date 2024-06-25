using FastEndpoints;

namespace Frontline.Features.Session.Inventory.Dropships.GetDropships;

public class Endpoint : Endpoint<GetInventoryRequest, List<object>>
{
    public override void Configure()
    {
        Get("/session/dropships");
    }

    public override async Task HandleAsync(GetInventoryRequest req, CancellationToken ct)
    {
        var response = new List<object>();
        
        await SendAsync(response, cancellation: ct);
    }
}