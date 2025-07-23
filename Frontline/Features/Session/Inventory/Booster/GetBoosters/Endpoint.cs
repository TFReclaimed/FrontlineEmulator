using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;

namespace Frontline.Features.Session.Inventory.Booster.GetBoosters;

public class Endpoint : Endpoint<GetInventoryRequest, List<int>>
{
    private readonly IPlayerRepository _playerRepository;

    public Endpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Get("/session/booster");
    }

    public override async Task HandleAsync(GetInventoryRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to get their booster packs but the profile wasn't found!", userId);
            await Send.NotFoundAsync();
            return;
        }
        
        // The client expects a list of integers representing the booster pack IDs.
        // Not sure why they wouldn't just return the amount of booster packs the player has.
        // Maybe the old backend allowed the developers to configure better items for more expensive booster packs?
        // Or maybe they planned to add more types of booster packs in the future?
        var response = new List<int>();
        for (var i = 0; i < player.BoosterPackCount; i++)
        {
            response.Add(i);
        }
        
        await Send.OkAsync(response);
    }
}