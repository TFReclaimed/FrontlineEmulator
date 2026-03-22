using System.Text.Json.Serialization;

namespace Frontline.Battle;

public class EventLogTraitCardInfo : ActiveTraitCardInfo
{
    [JsonPropertyName("effectID")]
    public int EffectId { get; set; }

    [JsonPropertyName("traitID")]
    public int TraitId { get; set; }

    public sbyte Data { get; set; }
}