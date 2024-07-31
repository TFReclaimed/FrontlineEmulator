using System.Text.Json.Serialization;

namespace Frontline.Game;

public class Ruleset
{
    public CardsRuleset CardsRuleset { get; set; }
    public CardXpRanks PilotXpRanksRuleset { get; set; }
    public CardXpRanks TitanXpRanksRuleset { get; set; }
}

public class CardsRuleset
{
    public Dictionary<string, CardTemplate> Cards { get; set; }
}

[JsonDerivedType(typeof(CardTemplate), "CardTemplate")]
[JsonDerivedType(typeof(UnitCardTemplate), "UnitTemplate")]
[JsonDerivedType(typeof(CommanderCardTemplate), "CommanderTemplate")]
[JsonDerivedType(typeof(ResourceCardTemplate), "ResourceCardTemplate")]
public class CardTemplate
{
    public int CardId { get; set; }
    public CardRarity Rarity { get; set; }
    public CardType Type { get; set; }
    public sbyte Cost { get; set; }
    [JsonPropertyName("rank")]
    public byte MinimumRank { get; set; }

    public int CreditsIfRetired(int xp)
    {
        var credits = Type switch
        {
            CardType.Pilot or CardType.Titan => 25 * (int) Math.Floor(xp * 0.2f),
            CardType.Support or CardType.BurnCard or CardType.Secret => 25,
            CardType.Commander => 300,
            _ => 0
        };

        credits = Rarity switch
        {
            CardRarity.Rare => (int) Math.Floor(credits * 1.25f),
            CardRarity.UltraRare => (int) Math.Floor(credits * 1.5f),
            CardRarity.Exclusive => (int) Math.Floor(credits * 2f),
            _ => credits
        };

        return credits;
    }

    public int XpIfRetired(int xp)
    {
        if (!IsCombatUnit())
        {
            return 0;
        }

        var receivedXp = 50 + (int) Math.Floor(xp * 0.5f);
        receivedXp = Rarity switch
        {
            CardRarity.Rare => (int) Math.Floor(receivedXp * 1.25f),
            CardRarity.UltraRare => (int) Math.Floor(receivedXp * 1.5f),
            CardRarity.Exclusive => (int) Math.Floor(receivedXp * 2f),
            _ => receivedXp
        };

        return receivedXp;
    }

    public bool IsCombatUnit()
    {
        return Type is CardType.Pilot or CardType.Titan;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CardRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    UltraRare = 3,
    Exclusive = 4,
    NumRarities = 5
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CardType
{
    Pilot = 0,
    Titan = 1,
    Support = 2,
    BurnCard = 3,
    Secret = 4,
    Commander = 5,
    Resource = 6,
    NumTypes = 7
}

public class UnitCardTemplate : CardTemplate
{
}

public class CommanderCardTemplate : CardTemplate
{
}

public class ResourceCardTemplate : CardTemplate
{
    public int ResourceValue { get; set; }
    public ResourceType ResourceType { get; set; }
}

public class CardXpRanks
{
    [JsonPropertyName("XPRanks")]
    public List<CardXpEntry> XpRanks { get; set; }
}

public class CardXpEntry
{
    [JsonPropertyName("Rank")]
    public int Rank { get; set; }
    [JsonPropertyName("XPRequired")]
    public int XpRequired { get; set; }
}