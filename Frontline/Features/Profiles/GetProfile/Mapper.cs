using FastEndpoints;
using Frontline.Data.Entities;

namespace Frontline.Features.Profiles.GetProfile;

public class Mapper : Mapper<GetProfileRequest, ProfileDetails, PlayerEntity>
{
    public override ProfileDetails FromEntity(PlayerEntity e)
    {
        return new ProfileDetails
        {
            ProfileId = e.Id,
            UserId = e.Id,
            DisplayName = e.Name,
            AvatarId = e.AvatarId,
            GameProfiles =
            [
                new GameProfile
                {
                    DropshipId = e.DropshipId,
                    Credits = e.Credits,
                    Supply = e.Supply,
                    Trophies = e.Trophies,
                    Tokens = e.Tokens,
                    Wins = e.Wins,
                    HighestTrophies = e.HighestTrophies,
                    MissionsComplete = e.MissionsComplete,
                    MatchesPlayed = e.MatchesPlayed,
                    Xp = e.Xp
                }
            ]
        };
    }
}