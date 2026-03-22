using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class ApplyHealMultiply : ApplyHeal
{
    public required TraitTargeting CountInfo { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var num = CountInfo.CalculateCount(GameState, active);
        var delta = (sbyte) (Heal * num);
        if (active.DataValue > 0)
        {
            delta = (sbyte) (active.DataValue * num);
        }

        if (card.GetCurrentHealth(false) > 0)
        {
            delta = card.HealDamage(null, delta);
        }

        if (delta <= 0)
        {
            return;
        }

        var traumaEvent = new CardTraumaCcgEvent(CcgEventType.CardHeal, delta, source.InstanceId,
            source.ActiveData.Owner, card.InstanceId, card.ActiveData.Owner);
        GameState.AddCcgEventLog(traumaEvent);
    }
}