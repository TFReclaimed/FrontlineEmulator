using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CcgEventType
{
    DeployUnit,
    DeployBurn,
    DeploySecret,
    Move,
    Disembark,
    TraitActivation,
    TraitExpendCharge,
    CombatStart,
    CombatBuffsAttack,
    CombatBuffsBypass,
    CombatBuffsArmor,
    CombatBuffsHealth,
    CombatBuffsConversion,
    CombatAttack,
    CombatCounter,
    CombatEnd,
    CardDamage,
    CardDeath,
    CardHeal,
    MulliganDraw,
    DeckDraw,
    SupportDraw,
    CardSummon,
    CardUnsummon,
    CardDiscard,
    SecretTriggered,
    TraitEvent,
    [JsonStringEnumMemberName("CardXPEarned")]
    CardXpEarned,
    NewTurn,
    EndTurn,
    ServerError,
    ServerRandomInt,
    [JsonStringEnumMemberName("ServerNewID")]
    ServerNewId,
    NumTypes
}