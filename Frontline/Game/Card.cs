using System.Text.Json.Serialization;

namespace Frontline.Game;

[JsonDerivedType(typeof(CommanderCard))]
[JsonDerivedType(typeof(ResourceCard))]
public class Card : Item
{
    public ActiveCardData? ActiveData { get; set; }
    public int Xp { get; set; }
    public sbyte Rank { get; set; } = 1;
}

public class ActiveCardData
{
    public required List<ActiveTrait> ActiveTraits { get; set; }
    public required bool[] TraitActivated { get; set; }
    public sbyte Owner { get; set; }
}

public class ActiveTrait
{
    public required TraitDuration DurationData { get; set; }
    public int TraitSourceId { get; set; }
    public int TraitEffectId { get; set; }
    public int DataValue { get; set; }
    public required ActiveTraitCardInfo Source { get; set; }
    public required ActiveTraitCardInfo Target { get; set; }
    public bool Detered { get; set; }
    public bool Triggered { get; set; }
}

public class TraitDuration
{
    public TraitDurationType Type { get; set; }
    public sbyte Duration { get; set; }
    public sbyte Charges { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TraitDurationType
{
    Instant = 0,
    Permanent = 1,
    EndOfTurn = 2,
    EndOfMyTurn = 3,
    EndOfEnemyTurn = 4,
    StartOfTurn = 5,
    StartOfMyTurn = 6,
    StartOfEnemyTurn = 7,
    NumDurations = 8
}

public class ActiveTraitCardInfo
{
    public int InstanceId { get; set; }
    public sbyte Owner { get; set; }
}

public class CommanderCard : Card
{
    [JsonPropertyName("$type")]
    public string Type { get; set; } = "1";
    public sbyte Defense { get; set; }
    public required List<Card> Secrets { get; set; }
}

public class ResourceCard : Card
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