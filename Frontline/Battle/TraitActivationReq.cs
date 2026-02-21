using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TraitActivationReq : byte
{
    None = 0,
    Targeted = 1,
    SelectFromDiscard = 2,
    NumReqTypes = 3
}