using System.Text.Json.Serialization;

namespace Frontline.Game;

[JsonDerivedType(typeof(ResourceCard))]
public class Card : Item
{
    public int Xp { get; set; }
    public sbyte Rank { get; set; } = 1;
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