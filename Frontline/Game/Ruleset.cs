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
            CardType.Pilot or CardType.Titan => 25 + (int) Math.Floor(xp * 0.2f),
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
    Common,
    Uncommon,
    Rare,
    UltraRare,
    Exclusive,
    NumRarities
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CardType
{
    Pilot,
    Titan,
    Support,
    BurnCard,
    Secret,
    Commander,
    Resource,
    NumTypes
}

public class UnitCardTemplate : CardTemplate
{
    public UnitType UnitType { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnitType
{
    None,
    Light,
    Medium,
    Heavy,
    Stryder,
    Atlas,
    Ogre,
    Installation,
    Commander,
    Spectre,
    NumTypes
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
    Credit,
    Xp,
    Supply,
    Token,
    Intel,
    Ticket,
    ReputationMilitiaHarmony,
    ReputationMilitiaKraken,
    [JsonStringEnumMemberName("ReputationIMCHarmony")]
    ReputationImcHarmony,
    [JsonStringEnumMemberName("ReputationIMCKraken")]
    ReputationImcKraken,
    ReputationFiller01,
    ReputationFiller02,
    ReputationFiller03,
    ReputationFiller04,
    ReputationFiller05,
    ReputationFiller06,
    ReputationFiller07,
    ReputationFiller08,
    IntelTypeOperational,
    IntelTypeTechnical,
    IntelTypePersonnel,
    IntelTypeAlien,
    IntelTypeFiller01,
    IntelTypeFiller02,
    IntelTypeFiller03,
    IntelTypeFiller04,
    NumResourceTypes
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