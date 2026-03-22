using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter<TraitTargetType>))]
public enum TraitTargetType
{
    Pilot,
    Titan,
    Support,
    BurnCard,
    Secret,
    Commander,
    Hard,
    Soft,
    Light,
    Medium,
    Heavy,
    Stryder,
    Atlas,
    Ogre,
    Spectre,
    Installation,
    [JsonStringEnumMemberName("CardID")]
    CardId,
    AnyType
}