using Frontline.Data.Entities;

namespace Frontline.Endpoints.Data.Leaderboards.GetPvpLeaderboard;

public class LeaderboardPvpResponse
{
    public string TournamentName { get; set; } = string.Empty;
    public required List<LeaderboardPvpEntry> Entries { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class LeaderboardPvpEntry
{
    public string Name { get; set; } = string.Empty;
    public string GuildName { get; set; } = string.Empty;
    public int Trophies { get; set; }
    public string Avatar { get; set; } = string.Empty;
    public int Id { get; set; }

    public static LeaderboardPvpEntry FromEntity(PlayerEntity entity)
    {
        return new LeaderboardPvpEntry
        {
            Name = entity.Name,
            GuildName = entity.GuildName,
            Trophies = entity.Trophies,
            Avatar = entity.AvatarId,
            Id = entity.Id
        };
    }
}