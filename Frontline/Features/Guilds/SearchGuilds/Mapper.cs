using FastEndpoints;
using Frontline.Data.Entities;

namespace Frontline.Features.Guilds.SearchGuilds;

public class Mapper : Mapper<SearchGuildRequest, List<GuildProfile>, List<GuildEntity>>
{
    public override List<GuildProfile> FromEntity(List<GuildEntity> e)
    {
        return e.Select(g => new GuildProfile
        {
            Id = g.Id.ToString(),
            Name = g.Name,
            Description = g.Description,
            AvatarId = g.AvatarId,
            Mode = g.Mode,
            Locale = g.Locale,
            MemberCount = g.Members.Count,
            MaxNumberOfMembers = g.MaxNumberOfMembers,
            Members = [] // Not used by the game
        }).ToList();
    }
}