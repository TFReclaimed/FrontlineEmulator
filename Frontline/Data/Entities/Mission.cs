using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(MissionKey))]
public class FinishedMissionEntity
{
    public int UserId { get; set; }
    [MaxLength(18)]
    public required string MissionKey { get; set; }
    [ForeignKey("UserId")]
    public PlayerEntity? Player { get; set; }
}

[PrimaryKey(nameof(UserId), nameof(MissionKey))]
public class ActiveMissionEntity
{
    public int UserId { get; set; }
    [MaxLength(18)]
    public required string MissionKey { get; set; }
    [ForeignKey("UserId")]
    public PlayerEntity? Player { get; set; }
    public DateTime Start { get; set; }
    public int RequiredCardItemId { get; set; }
    public int? BonusCard1ItemId { get; set; }
    public int? BonusCard2ItemId { get; set; }
    [ForeignKey("UserId,RequiredCardItemId")]
    public ItemEntity? RequiredCardItem { get; set; }
    [ForeignKey("UserId,BonusCard1ItemId")]
    public ItemEntity? BonusCard1Item { get; set; }
    [ForeignKey("UserId,BonusCard2ItemId")]
    public ItemEntity? BonusCard2Item { get; set; }
    public bool Success { get; set; }
    public bool Bonus1Success { get; set; }
    public bool Bonus2Success { get; set; }
    public bool Casualty { get; set; }
    public bool Bonus1Casualty { get; set; }
    public bool Bonus2Casualty { get; set; }
}