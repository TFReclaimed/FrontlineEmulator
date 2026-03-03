using System.Text.Json.Serialization;

namespace Frontline.Endpoints.Data.Leaderboards.GetPveLeaderboard;

public class LeaderboardPveResponse
{
    public string TournamentName { get; set; } = string.Empty;
    public required List<LeaderboardPveEntry> Entries { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class LeaderboardPveEntry
{
    public Territory Territory { get; set; }
    public LeaderboardFaction Faction { get; set; }
    public int Rank { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<Territory>))]
public enum Territory
{
    Harmony,
    Kraken
}

[JsonConverter(typeof(JsonStringEnumConverter<LeaderboardFaction>))]
public enum LeaderboardFaction
{
    [JsonStringEnumMemberName("IMC")]
    Imc,
    Militia
}