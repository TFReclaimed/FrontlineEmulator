using FastEndpoints;

namespace Frontline.Features.Session.Inventory.GetInventory;

public class Endpoint : Endpoint<GetInventoryRequest, string>
{
    public override void Configure()
    {
        Get("/session/inventory");
    }

    public override async Task HandleAsync(GetInventoryRequest req, CancellationToken ct)
    {
        var json = "{\"$types\":{\"InventoryList\":\"1\",\"CommanderCard\":\"2\"},\"$type\":\"1\",\"bundleData\":[],\"items\":[{\"$type\":\"2\",\"defense\":0,\"secrets\":[],\"activeData\":null,\"xp\":0,\"rank\":1,\"availability\":null,\"instanceId\":0,\"templateId\":282,\"gameData\":null,\"bundle\":\"PortraitBase\"}],\"includesLast\":false}";
        
        await SendStringAsync(json, contentType: "application/json", cancellation: ct);
    }
}