using System.Text.Json.Serialization;

namespace Frontline.Features.Profiles;

public class PlayerProfile
{
    public string SessionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int CharacterId { get; set; }
    public required ProfileDetails Details { get; set; }
}

public class ProfileDetails
{
    public int ProfileId { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarId { get; set; } = string.Empty;
    public required List<GameProfile> GameProfiles { get; set; }
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
}