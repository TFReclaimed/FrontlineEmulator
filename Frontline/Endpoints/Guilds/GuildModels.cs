using Frontline.Data.Entities;

namespace Frontline.Endpoints.Guilds;

public class GuildProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AvatarId { get; set; } = string.Empty;
    public GuildMode Mode { get; set; }
    public GuildLocale Locale { get; set; }
    public required GuildDetails Details { get; set; }
    public int MemberCount { get; set; }
    public int MaxNumberOfMembers { get; set; }
    public required List<GuildMemberDto> Members { get; set; }

    public static GuildProfileDto FromEntity(GuildEntity entity)
    {
        return new GuildProfileDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            AvatarId = entity.AvatarId,
            Mode = entity.Mode,
            Locale = entity.Locale,
            Details = new GuildDetails
            {
                GameProfiles =
                [
                    new GuildData
                    {
                        Trophies = entity.Members.Sum(m => m.Player?.Trophies ?? 0)
                    }
                ]
            },
            MemberCount = entity.Members.Count,
            MaxNumberOfMembers = entity.MaxNumberOfMembers,
            Members = entity.Members.Select(GuildMemberDto.FromEntity).ToList()
        };
    }
}

public class GuildDetails
{
    public required List<GuildData> GameProfiles { get; set; }
}

public class GuildData
{
    public int Trophies { get; set; }
}

public class GuildMemberDto
{
    public string MemberId { get; set; } = string.Empty;
    public MemberRank Rank { get; set; }

    public static GuildMemberDto FromEntity(GuildMemberEntity entity)
    {
        return new GuildMemberDto
        {
            MemberId = entity.UserId.ToString(),
            Rank = entity.Rank
        };
    }
}