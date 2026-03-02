using System.Text.Json.Serialization;

namespace Frontline.Game.Card;

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