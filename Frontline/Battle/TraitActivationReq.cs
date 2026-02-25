using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TraitActivationReq
{
    None,
    Targeted,
    SelectFromDiscard,
    NumReqTypes
}