using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class ApplyHealMultiply : ApplyHeal
{
    public required TraitTargeting CountInfo { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var num = CountInfo.CalculateCount(GameState, active);
        var b = (sbyte) (Heal * num);
        if (active.DataValue > 0)
        {
            b = (sbyte) (active.DataValue * num);
        }

        if (card.GetCurrentHealth(false) > 0)
        {
            b = card.HealDamage(null, b);
        }

        if (b > 0)
        {
            var logData = new CardTraumaCcgEvent(CcgEventType.CardHeal, b, source.InstanceId,
                source.ActiveData.Owner, card.InstanceId, card.ActiveData.Owner);
            GameState.AddCCGEventLog(logData);
        }
    }
}