using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Frontline.Data.Entities;

public class GuildEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [MaxLength(18)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(17)]
    public string AvatarId { get; set; } = string.Empty;
    public GuildMode Mode { get; set; }
    public GuildLocale Locale { get; set; }
    public int MaxNumberOfMembers { get; set; } = 50;
    
    public ICollection<GuildMemberEntity> Members { get; set; } = new List<GuildMemberEntity>();
}

public class GuildMemberEntity
{
    [Key, Column(Order = 0)]
    public int UserId { get; set; }
    [Column(Order = 1)]
    public Guid GuildId { get; set; }
    [ForeignKey("UserId")]
    public PlayerEntity? Player { get; set; }
    [ForeignKey("GuildId")]
    public GuildEntity? Guild { get; set; }
    public MemberRank Rank { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<GuildMode>))]
public enum GuildMode
{
    [JsonStringEnumMemberName("PUBLIC")]
    Public,
    [JsonStringEnumMemberName("PRIVATE")]
    Private
}

[JsonConverter(typeof(JsonStringEnumConverter<GuildLocale>))]
public enum GuildLocale
{
    None,
    [JsonStringEnumMemberName("USA")]
    UnitedStates,
    [JsonStringEnumMemberName("DEU")]
    Germany,
    [JsonStringEnumMemberName("FRA")]
    France,
    [JsonStringEnumMemberName("ESP")]
    Spain,
    [JsonStringEnumMemberName("ITA")]
    Italy,
    [JsonStringEnumMemberName("PRT")]
    Portugal,
    [JsonStringEnumMemberName("RUS")]
    Russia,
    [JsonStringEnumMemberName("KOR")]
    Korea,
    [JsonStringEnumMemberName("JPN")]
    Japan,
    [JsonStringEnumMemberName("CHN")]
    China,
    [JsonStringEnumMemberName("THA")]
    Thailand,
    [JsonStringEnumMemberName("afghanistan")]
    Afghanistan,
    [JsonStringEnumMemberName("brazil")]
    Brazil,
    [JsonStringEnumMemberName("canada")]
    Canada,
    [JsonStringEnumMemberName("england")]
    UnitedKingdom,
    [JsonStringEnumMemberName("finland")]
    Finland,
    [JsonStringEnumMemberName("mexico")]
    Mexico,
    [JsonStringEnumMemberName("sweden")]
    Sweden
}

[JsonConverter(typeof(JsonStringEnumConverter<MemberRank>))]
public enum MemberRank
{
    [JsonStringEnumMemberName("INVALID")]
    Invalid,
    [JsonStringEnumMemberName("MEMBER")]
    Member,
    [JsonStringEnumMemberName("OFFICER")]
    Officer,
    [JsonStringEnumMemberName("LEADER")]
    Leader
}