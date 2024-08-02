using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Repositories;

namespace Frontline.Features.Profiles.UpdateGameProfile;

public class Endpoint : Endpoint<GameProfileUpdateRequest>
{
    private readonly IPlayerRepository _playerRepository;

    public Endpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Put("/virtu2/profiles/{UserId}/private/game");
    }

    public override async Task HandleAsync(GameProfileUpdateRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.ClaimValue("UserId")!);
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to update their game profile but it wasn't found!", userId);
            await SendNotFoundAsync();
            return;
        }

        if ((req.ActiveDeckId == 1 && player.Level < 2) || (req.ActiveDeckId is 10 or 11 && player.Level < 3))
        {
            Logger.LogWarning("Player {UserId} tried to select a dropship they haven't unlocked yet!", userId);
            await SendForbiddenAsync();
            return;
        }
        
        Logger.LogInformation("Updated game profile for user {UserId}. New dropship ID: {DropshipId}",
            userId, req.ActiveDeckId);
        
        player.DropshipId = req.ActiveDeckId;
        await _playerRepository.UpdatePlayerAsync(player);
        
        await SendOkAsync();
    }
}