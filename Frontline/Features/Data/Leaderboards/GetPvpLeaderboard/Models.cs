namespace Frontline.Features.Data.Leaderboards.GetPvpLeaderboard;

public class LeaderboardPvpResponse
{
    public string TournamentName { get; set; }
    public List<LeaderboardPvpEntry> Entries { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class LeaderboardPvpEntry
{
    public string Name { get; set; }
    public string GuildName { get; set; }
    public int Trophies { get; set; }
    public string Avatar { get; set; }
    public int Id { get; set; }
}