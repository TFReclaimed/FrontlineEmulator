using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CCGEventType
{
    DeployUnit = 0,
    DeployBurn = 1,
    DeploySecret = 2,
    Move = 3,
    Disembark = 4,
    TraitActivation = 5,
    TraitExpendCharge = 6,
    CombatStart = 7,
    CombatBuffsAttack = 8,
    CombatBuffsBypass = 9,
    CombatBuffsArmor = 10,
    CombatBuffsHealth = 11,
    CombatBuffsConversion = 12,
    CombatAttack = 13,
    CombatCounter = 14,
    CombatEnd = 15,
    CardDamage = 16,
    CardDeath = 17,
    CardHeal = 18,
    MulliganDraw = 19,
    DeckDraw = 20,
    SupportDraw = 21,
    CardSummon = 22,
    CardUnsummon = 23,
    CardDiscard = 24,
    SecretTriggered = 25,
    TraitEvent = 26,
    CardXPEarned = 27,
    NewTurn = 28,
    EndTurn = 29,
    ServerError = 30,
    ServerRandomInt = 31,
    ServerNewID = 32,
    NumTypes = 33
}