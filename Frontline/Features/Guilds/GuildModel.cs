using System.Text.Json.Serialization;
using Frontline.Data.Entities;

namespace Frontline.Features.Guilds;

public class GuildProfile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AvatarId { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GuildMode Mode { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GuildLocale Locale { get; set; }
    public int MemberCount { get; set; }
    public int MaxNumberOfMembers { get; set; }
    public required List<GuildMember> Members { get; set; }
}

public class GuildMember
{
    public string MemberId { get; set; } = string.Empty;
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MemberRank Rank { get; set; }
}