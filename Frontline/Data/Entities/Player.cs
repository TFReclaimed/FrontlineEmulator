using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Frontline.Data.Entities;

public class PlayerEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    [MaxLength(18)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(10)]
    public string AvatarId { get; set; } = string.Empty;
    public int DropshipId { get; set; }
    public int Credits { get; set; }
    public int Supply { get; set; }
    public int Trophies { get; set; }
    public int Tokens { get; set; }
    public int Wins { get; set; }
    public int HighestTrophies { get; set; }
    public int MissionsComplete { get; set; }
    public int MatchesPlayed { get; set; }
    public int Xp { get; set; }
    public int BoosterPackCount { get; set; }
    public DateTime LastGiftSent { get; set; }
    
    [NotMapped]
    public string GuildName { get; set; } = string.Empty;

    public int Level => (int) Math.Sqrt(Xp / 125f);
}