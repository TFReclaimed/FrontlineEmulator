using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Frontline.Features.Profiles.UpdateProfile;

public class Endpoint : Endpoint<ProfileUpdateRequest, Ok>
{
    private readonly IPlayerRepository _playerRepository;

    public Endpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Put("/virtu2/profiles/{UserId}/private");
    }

    public override async Task HandleAsync(ProfileUpdateRequest req, CancellationToken ct)
    {
        var userId = this.GetUserId();
        var player = await _playerRepository.GetPlayerAsync(userId);
        if (player is null)
        {
            Logger.LogWarning("Player {UserId} tried to update their profile but it wasn't found!", userId);
            await Send.NotFoundAsync();
            return;
        }
        
        Logger.LogInformation("Updated profile for user {UserId}. New name: {Name}, New avatar: {AvatarId}",
            userId, req.DisplayName, req.AvatarId);
        
        player.Name = req.DisplayName;
        player.AvatarId = req.AvatarId;
        
        await _playerRepository.UpdatePlayerAsync(player);
        
        await Send.OkAsync();
    }
}