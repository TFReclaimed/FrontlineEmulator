using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TriggerType
{
    None = 0,
    TakeDamage = 1,
    Destroy = 2,
    Attack = 3,
    CounterAttack = 4,
    Move = 5,
    ActivateSkill = 6,
    Deploy = 7,
    Hack = 8,
    SecretTrigger = 9,
    SecretDestroy = 10,
    NewTurn = 11,
    EndTurn = 12,
    DeckDraw = 13,
    BonusDeckDraw = 14,
    SupportDraw = 15,
    BonusSupportDraw = 16,
    Discard = 17,
    ActivateDamageEffect = 18,
    ActivateHealEffect = 19,
    NumTriggerTypes = 20
}