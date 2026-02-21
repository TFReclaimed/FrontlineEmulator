using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class ApplyHealMultiply : ApplyHeal
{
    public TraitTargeting countInfo;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        int num = countInfo.CalculateCount(GameState, active);
        sbyte b = (sbyte) (heal * num);
        if (active.dataValue > 0)
        {
            b = (sbyte) (active.dataValue * num);
        }

        if (card.GetCurrentHealth(false) > 0)
        {
            b = card.HealDamage(null, b);
        }

        if (b > 0)
        {
            CardTraumaCCGEvent logData = new CardTraumaCCGEvent(CCGEventType.CardHeal, b, source.instanceId,
                source.activeData.owner, card.instanceId, card.activeData.owner);
            GameState.AddCCGEventLog(logData);
        }
    }
}