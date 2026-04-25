using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Frontline.Data.Entities;

public class ChatMessageEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [MaxLength(41)]
    public string Room { get; set; } = string.Empty;
    public int SenderId { get; set; }
    [ForeignKey("SenderId")]
    public PlayerEntity? Player { get; set; }
    [MaxLength(140)]
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}