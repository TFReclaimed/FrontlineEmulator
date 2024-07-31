using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(ItemId))]
public class ItemEntity
{
    public int UserId { get; set; }
    public int ItemId { get; set; }
    [ForeignKey("UserId")]
    public PlayerEntity Player { get; set; }
    public int TemplateId { get; set; }
    public int Xp { get; set; }
    public sbyte Rank { get; set; } = 1;
    public bool Casualty { get; set; }
    
    [NotMapped]
    public string? CurrentMission { get; set; }
}