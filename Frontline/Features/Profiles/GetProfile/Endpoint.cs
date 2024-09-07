using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;

namespace Frontline.Features.Profiles.GetProfile;

public class Endpoint : Endpoint<GetProfileRequest, ProfileDetails, Mapper>
{
    private readonly IPlayerRepository _playerRepository;

    public Endpoint(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public override void Configure()
    {
        Get("/virtu2/profiles/{UserId}/{ProfileType}");
    }

    public override async Task HandleAsync(GetProfileRequest req, CancellationToken ct)
    {
        var userId = req.ProfileType == ProfileType.Public ? req.UserId : this.GetUserId();
        if (userId == -1)
        {
            var systemProfile = new ProfileDetails
            {
                ProfileId = -1,
                UserId = -1,
                DisplayName = "<color=red>SYSTEM</color>",
                AvatarId = "avatar006",
                GameProfiles =
                [
                    new GameProfile()
                ]
            };
            
            await SendAsync(systemProfile);
            return;
        }
        
        var entity = await _playerRepository.GetPlayerAsync(userId);
        if (entity is null)
        {
            await SendNotFoundAsync();
            return;
        }
        
        var profile = Map.FromEntity(entity);
        
        await SendAsync(profile);
    }
}