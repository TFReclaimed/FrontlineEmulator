using System.Text.Json.Serialization;
using Frontline.Data.Entities;

namespace Frontline.Endpoints.Profiles;

public class PlayerProfile
{
    public string SessionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int CharacterId { get; set; }
    public required ProfileDetails Details { get; set; }

    public static PlayerProfile FromEntity(PlayerEntity entity)
    {
        return new PlayerProfile
        {
            Name = entity.Name,
            UserId = entity.Id,
            CharacterId = entity.Id,
            Details = ProfileDetails.FromEntity(entity)
        };
    }
}

public class ProfileDetails
{
    public int ProfileId { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarId { get; set; } = string.Empty;
    public required List<GameProfile> GameProfiles { get; set; }

    public static ProfileDetails FromEntity(PlayerEntity entity)
    {
        return new ProfileDetails
        {
            ProfileId = entity.Id,
            UserId = entity.Id,
            DisplayName = entity.Name,
            AvatarId = entity.AvatarId,
            GameProfiles =
            [
                GameProfile.FromEntity(entity)
            ]
        };
    }
}

public class GameProfile
{
    [JsonPropertyName("activeDeckId")]
    public int DropshipId { get; set; }
    public int Credits { get; set; }
    public int Supply { get; set; }
    public int Trophies { get; set; }
    public int Tokens { get; set; }
    public int Wins { get; set; }
    public int HighestTrophies { get; set; }
    public int MissionsComplete { get; set; }
    public int CardsCollected { get; set; }
    public int MatchesPlayed { get; set; }
    public int Xp { get; set; }

    public static GameProfile FromEntity(PlayerEntity entity)
    {
        return new GameProfile
        {
            DropshipId = entity.DropshipId,
            Credits = entity.Credits,
            Supply = entity.Supply,
            Trophies = entity.Trophies,
            Tokens = entity.Tokens,
            Wins = entity.Wins,
            HighestTrophies = entity.HighestTrophies,
            MissionsComplete = entity.MissionsComplete,
            MatchesPlayed = entity.MatchesPlayed,
            Xp = entity.Xp
        };
    }
}