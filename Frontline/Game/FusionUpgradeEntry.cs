namespace Frontline.Game;

public class FusionUpgradeEntry
{
    public int CardTemplateId { get; set; }
    public int Rank { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Command { get; set; }
    public int Attack { get; set; }
    public int Health { get; set; }
    public int TraitId { get; set; }
    public int Armor { get; set; }
}