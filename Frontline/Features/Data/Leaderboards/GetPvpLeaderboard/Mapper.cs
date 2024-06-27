using FastEndpoints;
using Frontline.Data.Entities;

namespace Frontline.Features.Data.Leaderboards.GetPvpLeaderboard;

public class Mapper : ResponseMapper<List<LeaderboardPvpEntry>, List<PlayerEntity>>
{
    public override List<LeaderboardPvpEntry> FromEntity(List<PlayerEntity> e)
    {
        return e.Select(p => new LeaderboardPvpEntry
        {
            Name = p.Name,
            GuildName = p.GuildName,
            Trophies = p.Trophies,
            Avatar = p.AvatarId,
            Id = p.Id
        }).ToList();
    }
}