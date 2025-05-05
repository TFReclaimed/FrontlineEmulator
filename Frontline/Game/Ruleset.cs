using System.Text.Json.Serialization;

namespace Frontline.Game;

public class Ruleset
{
    public required CardsRuleset CardsRuleset { get; set; }
    public required CardXpRanks PilotXpRanksRuleset { get; set; }
    public required CardXpRanks TitanXpRanksRuleset { get; set; }
}

public class CardsRuleset
{
    public required Dictionary<string, CardTemplate> Cards { get; set; }
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
    public UnitType UnitType { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnitType
{
    None = 0,
    Light = 1,
    Medium = 2,
    Heavy = 3,
    Stryder = 4,
    Atlas = 5,
    Ogre = 6,
    Installation = 7,
    Commander = 8,
    Spectre = 9,
    NumTypes = 10
}

public class CommanderCardTemplate : CardTemplate
{
    public required List<int> SupportIds { get; set; }
}

public class ResourceCardTemplate : CardTemplate
{
    public int ResourceValue { get; set; }
    public ResourceType ResourceType { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceType
{
    Credit = 0,
    Xp = 1,
    Supply = 2,
    Token = 3,
    Intel = 4,
    Ticket = 5,
    ReputationMilitiaHarmony = 6,
    ReputationMilitiaKraken = 7,
    ReputationIMCHarmony = 8,
    ReputationIMCKraken = 9,
    ReputationFiller01 = 10,
    ReputationFiller02 = 11,
    ReputationFiller03 = 12,
    ReputationFiller04 = 13,
    ReputationFiller05 = 14,
    ReputationFiller06 = 15,
    ReputationFiller07 = 16,
    ReputationFiller08 = 17,
    IntelTypeOperational = 18,
    IntelTypeTechnical = 19,
    IntelTypePersonnel = 20,
    IntelTypeAlien = 21,
    IntelTypeFiller01 = 22,
    IntelTypeFiller02 = 23,
    IntelTypeFiller03 = 24,
    IntelTypeFiller04 = 25,
    NumResourceTypes = 26
}

public class CardXpRanks
{
    [JsonPropertyName("XPRanks")]
    public required List<CardXpEntry> XpRanks { get; set; }
}

public class CardXpEntry
{
    [JsonPropertyName("Rank")]
    public int Rank { get; set; }
    [JsonPropertyName("XPRequired")]
    public int XpRequired { get; set; }
}