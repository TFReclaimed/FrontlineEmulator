using FastEndpoints;
using Frontline.Data.Entities;

namespace Frontline.Features.Profiles.Login;

public class Mapper : Mapper<LoginRequest, PlayerProfile, PlayerEntity>
{
    public override PlayerProfile FromEntity(PlayerEntity e)
    {
        return new PlayerProfile
        {
            Name = e.Name,
            UserId = e.Id,
            CharacterId = e.Id,
            Details = new ProfileDetails
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
                        CardsCollected = 1, // TODO
                        MatchesPlayed = e.MatchesPlayed,
                        Xp = e.Xp
                    }
                ]
            }
        };
    }
}