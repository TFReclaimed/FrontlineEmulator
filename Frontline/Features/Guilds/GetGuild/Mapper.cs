using FastEndpoints;
using Frontline.Data.Entities;

namespace Frontline.Features.Guilds.GetGuild;

public class Mapper : Mapper<GetGuildRequest, GuildProfile, GuildEntity>
{
    public override GuildProfile FromEntity(GuildEntity e)
    {
        return new GuildProfile
        {
            Id = e.Id.ToString(),
            Name = e.Name,
            Description = e.Description,
            AvatarId = e.AvatarId,
            Mode = e.Mode,
            Locale = e.Locale,
            MemberCount = e.Members.Count,
            MaxNumberOfMembers = e.MaxNumberOfMembers,
            Members = e.Members.Select(m => new GuildMember
            {
                MemberId = m.Player.Id.ToString(),
                Rank = m.Rank
            }).ToList()
        };
    }
}