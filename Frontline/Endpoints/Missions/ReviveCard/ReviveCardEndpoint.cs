using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;

namespace Frontline.Endpoints.Missions.ReviveCard;

public class ReviveCardEndpoint : Endpoint<ReviveCardRequest>
{
    private readonly IPlayerRepository _playerRepository;

    private readonly IInventoryRepository _inventoryRepository;

    public ReviveCardEndpoint(IPlayerRepository playerRepository, IInventoryRepository inventoryRepository)
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
        var userId = this.GetUserId();
        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player not found: {UserId}", userId);
            await Send.NotFoundAsync();
            return;
        }

        var item = await _inventoryRepository.GetItemAsync(player.Id, req.InstanceId);
        if (item is null)
        {
            Logger.LogWarning("Item not found: {InstanceId}", req.InstanceId);
            await Send.NotFoundAsync();
            return;
        }

        // Revive cost is hardcoded to 1 token
        const int reviveCost = 1;

        if (player.Tokens < reviveCost)
        {
            Logger.LogWarning("Player {UserId} doesn't have enough tokens to revive card {InstanceId}",
                userId, req.InstanceId);
            await Send.ResultAsync(TypedResults.BadRequest());
            return;
        }

        Logger.LogInformation("Player {UserId} revived card {InstanceId}", userId, req.InstanceId);

        player.Tokens -= reviveCost;
        await _playerRepository.UpdateAsync(player);

        item.Casualty = false;
        await _inventoryRepository.UpdateAsync(item);

        await Send.OkAsync();
    }
}