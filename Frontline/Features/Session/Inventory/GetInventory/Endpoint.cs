using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;

namespace Frontline.Features.Session.Inventory.GetInventory;

public class Endpoint : Endpoint<GetInventoryRequest, InventoryListResponse, Mapper>
{
    private readonly IInventoryRepository _inventoryRepository;

    public Endpoint(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public override void Configure()
    {
        Get("/session/inventory");
    }

    public override async Task HandleAsync(GetInventoryRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var items = _inventoryRepository.GetItems(userId, req.Param.MaxItem);

        var response = new InventoryListResponse
        {
            Items = Map.FromEntity(items)
        };
        
        await SendAsync(response);
    }
}