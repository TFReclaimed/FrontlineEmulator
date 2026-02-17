using FastEndpoints;
using Frontline.Data.Repositories;
using Frontline.Extensions;

namespace Frontline.Endpoints.Profiles.GetProfile;

public class GetProfileEndpoint : Endpoint<GetProfileRequest, ProfileDetails>
{
    private readonly IPlayerRepository _playerRepository;

    private readonly IInventoryRepository _inventoryRepository;

    public GetProfileEndpoint(IPlayerRepository playerRepository, IInventoryRepository inventoryRepository)
    {
        _playerRepository = playerRepository;
        _inventoryRepository = inventoryRepository;
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

            await Send.OkAsync(systemProfile);
            return;
        }

        var player = await _playerRepository.GetByIdAsync(userId);
        if (player is null)
        {
            await Send.NotFoundAsync();
            return;
        }

        var profile = ProfileDetails.FromEntity(player);
        profile.GameProfiles[0].CardsCollected = await _inventoryRepository.GetItemCountAsync(userId);

        await Send.OkAsync(profile);
    }
}