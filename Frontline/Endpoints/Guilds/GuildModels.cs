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
            MemberCount = entity.Members.Count,
            MaxNumberOfMembers = entity.MaxNumberOfMembers,
            Members = entity.Members.Select(GuildMemberDto.FromEntity).ToList()
        };
    }
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