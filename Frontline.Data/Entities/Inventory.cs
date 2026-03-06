using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(ItemId))]
public class ItemEntity
{
    public int UserId { get; set; }
    public int ItemId { get; set; }
    [ForeignKey("UserId")]
    public PlayerEntity? Player { get; set; }
    public int TemplateId { get; set; }
    public int Xp { get; set; }
    public sbyte Rank { get; set; } = 1;
    public bool Casualty { get; set; }

    [NotMapped]
    public int DropshipId { get; set; } = -1;
    [NotMapped]
    public bool IsInDropship => DropshipId != -1;
    [NotMapped]
    public string? CurrentMission { get; set; }
}

[PrimaryKey(nameof(UserId), nameof(DropshipId), nameof(SlotIndex))]
public class DropshipEntity
{
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public PlayerEntity? Player { get; set; }
    public int DropshipId { get; set; }
    public int SlotIndex { get; set; }
    public int ItemId { get; set; }
    [ForeignKey("UserId,ItemId")]
    public ItemEntity? Item { get; set; }
}