using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;

namespace Frontline.Endpoints.Profiles.UpdateGameProfile;

public class UpdateGameProfileEndpoint : Endpoint<GameProfileUpdateRequest>
{
    private readonly IPlayerRepository _playerRepository;

    public UpdateGameProfileEndpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Put("/virtu2/profiles/{UserId}/private/game");
    }

    public override async Task HandleAsync(GameProfileUpdateRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to update their game profile but it wasn't found!", userId);
            await Send.NotFoundAsync();
            return;
        }

        if (!CanUseDropship(req.ActiveDeckId, player.Level))
        {
            Logger.LogWarning("Player {UserId} tried to select a dropship they haven't unlocked yet!", userId);
            await Send.ForbiddenAsync();
            return;
        }
        
        Logger.LogInformation("Updated game profile for user {UserId}. New dropship ID: {DropshipId}",
            userId, req.ActiveDeckId);
        
        player.DropshipId = req.ActiveDeckId;
        await _playerRepository.UpdateAsync(player);
        
        await Send.OkAsync();
    }

    private static bool CanUseDropship(int dropshipId, int playerLevel)
    {
        return dropshipId switch
        {
            0 => true,
            1 => playerLevel >= 2,
            10 or 11 => playerLevel >= 3,
            12 => playerLevel >= 4,
            13 => playerLevel >= 5,
            14 => playerLevel >= 6,
            15 => playerLevel >= 7,
            16 => playerLevel >= 8,
            17 => playerLevel >= 9,
            _ => false
        };
    }
}