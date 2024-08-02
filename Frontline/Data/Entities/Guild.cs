using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

public enum GuildMode
{
    PUBLIC = 0,
    PRIVATE = 1
}

public enum GuildLocale
{
    NONE = 0,
    USA = 1,
    DEU = 2,
    FRA = 3,
    ESP = 4,
    ITA = 5,
    PRT = 6,
    RUS = 7,
    KOR = 8,
    JPN = 9,
    CHN = 10,
    THA = 11
}

public enum MemberRank
{
    INVALID = 0,
    MEMBER = 1,
    OFFICER = 2,
    LEADER = 3
}