using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class ApplyHealMultiply : ApplyHeal
{
    public TraitTargeting countInfo;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        int num = countInfo.CalculateCount(GameState, active);
        sbyte b = (sbyte) (heal * num);
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
            CardTraumaCCGEvent logData = new CardTraumaCCGEvent(CCGEventType.CardHeal, b, source.InstanceId,
                source.ActiveData.Owner, card.InstanceId, card.ActiveData.Owner);
            GameState.AddCCGEventLog(logData);
        }
    }
}