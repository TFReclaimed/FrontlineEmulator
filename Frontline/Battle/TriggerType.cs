using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter<TriggerType>))]
public enum TriggerType
{
    None,
    TakeDamage,
    Destroy,
    Attack,
    CounterAttack,
    Move,
    ActivateSkill,
    Deploy,
    Hack,
    SecretTrigger,
    SecretDestroy,
    NewTurn,
    EndTurn,
    DeckDraw,
    BonusDeckDraw,
    SupportDraw,
    BonusSupportDraw,
    Discard,
    ActivateDamageEffect,
    ActivateHealEffect,
    NumTriggerTypes
}