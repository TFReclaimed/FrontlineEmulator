using FastEndpoints;
using FastEndpoints.Security;
using Frontline.Data.Repositories;

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
        int userId;
        
        if (req.ProfileType == ProfileType.Public)
        {
            userId = req.UserId;
        }
        else
        {
            userId = int.Parse(User.ClaimValue("UserId")!);
        }
        
        var entity = await _playerRepository.GetPlayerAsync(userId);
        if (entity is null)
        {
            await SendResultAsync(TypedResults.NotFound());
            return;
        }
        
        var profile = Map.FromEntity(entity!);
        
        await SendAsync(profile, cancellation: ct);
    }
}