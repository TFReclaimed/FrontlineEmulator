using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Repositories;
using Frontline.Game;

namespace Frontline.Features.Session.Inventory.Booster.OpenBooster;

public class Endpoint : Endpoint<OpenBoosterPackRequest, BoosterPackResponse>
{
    private readonly IPlayerRepository _playerRepository;

    public Endpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Post("/session/booster/{BoosterId}");
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(OpenBoosterPackRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to open a booster pack but the profile wasn't found!", userId);
            await SendNotFoundAsync();
            return;
        }
        
        if (player.BoosterPackCount <= 0)
        {
            Logger.LogWarning("Player {UserId} tried to open a booster pack but they don't have any!", userId);
            await SendResultAsync(TypedResults.BadRequest());
            return;
        }

        player.BoosterPackCount--;
        await _playerRepository.UpdatePlayerAsync(player);

        var resourceCard = new ResourceCard
        {
            TemplateId = 639,
            ResourceValue = 0,
            ResourceType = ResourceType.Token
        };
        
        var response = new BoosterPackResponse
        {
            Cards = [],
            Resources = [resourceCard, resourceCard, resourceCard, resourceCard, resourceCard]
        };
        
        await SendAsync(response);
    }
}