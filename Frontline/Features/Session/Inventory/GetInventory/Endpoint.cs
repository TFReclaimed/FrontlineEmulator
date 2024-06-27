using FastEndpoints;
using Frontline.Game;

namespace Frontline.Features.Session.Inventory.GetInventory;

public class Endpoint : Endpoint<GetInventoryRequest, InventoryListResponse>
{
    public override void Configure()
    {
        Get("/session/inventory");
    }

    public override async Task HandleAsync(GetInventoryRequest req, CancellationToken ct)
    {
        var response = new InventoryListResponse
        {
            Items =
            [
                new CommanderCard
                {
                    Defense = 0,
                    Xp = 0,
                    Rank = 1,
                    TemplateId = 282,
                    AssetBundle = "PortraitBase"
                }
            ]
        };
        
        await SendAsync(response, cancellation: ct);
    }
}