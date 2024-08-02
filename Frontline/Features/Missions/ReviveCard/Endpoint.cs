using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Repositories;

namespace Frontline.Features.Missions.ReviveCard;

public class Endpoint : Endpoint<ReviveCardRequest>
{
    private readonly IPlayerRepository _playerRepository;
    
    private readonly IInventoryRepository _inventoryRepository;

    public Endpoint(IPlayerRepository playerRepository, IInventoryRepository inventoryRepository)
    {
        _playerRepository = playerRepository;
        _inventoryRepository = inventoryRepository;
    }

    public override void Configure()
    {
        Post("/Missions/v1/revivecard");
        AllowFormData(urlEncoded: true);
    }
    
    public override async Task HandleAsync(ReviveCardRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player not found: {UserId}", userId);
            await SendNotFoundAsync();
            return;
        }
        
        var item = await _inventoryRepository.GetItemAsync(player.Id, req.InstanceId);
        if (item is null)
        {
            Logger.LogWarning("Item not found: {InstanceId}", req.InstanceId);
            await SendNotFoundAsync();
            return;
        }

        // Revive cost is hardcoded to 1 token
        const int reviveCost = 1;
        
        if (player.Tokens < reviveCost)
        {
            Logger.LogWarning("Player {UserId} doesn't have enough tokens to revive card {InstanceId}",
                userId, req.InstanceId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }
        
        player.Tokens -= reviveCost;
        await _playerRepository.UpdatePlayerAsync(player);
        
        item.Casualty = false;
        await _inventoryRepository.UpdateItemAsync(item);
        
        await SendOkAsync();
    }
}