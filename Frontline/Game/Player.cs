namespace Frontline.Game;

public class Player
{
    public required Deck Deck { get; set; }
    public required SupportDeck SupportDeck { get; set; }
    public required CardCollection Hand { get; set; }
    public required CardCollection Discard { get; set; }
    public required GameResources Resources { get; set; }
    public required CardStack Commander { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UserId { get; set; }
    public bool Surrender { get; set; }
}

public class GameResources
{
    public sbyte CommandAccum { get; set; }
    public sbyte CommandUnits { get; set; }
    public sbyte Health { get; set; }
    public sbyte MaxHealth { get; set; }
    public sbyte DrawDamage { get; set; }
}