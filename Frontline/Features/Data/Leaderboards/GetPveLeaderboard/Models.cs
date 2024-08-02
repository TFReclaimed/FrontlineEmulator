using System.Text.Json.Serialization;

namespace Frontline.Features.Data.Leaderboards.GetPveLeaderboard;

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
    public Faction Faction { get; set; }
    public int Rank { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Territory
{
    Harmony = 0,
    Kraken = 1
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Faction
{
    IMC = 0,
    Militia = 1
}