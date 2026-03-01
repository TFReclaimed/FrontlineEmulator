namespace Frontline.Battle.Traits;

public class ForceDisembarkEffect : BaseTraitEffect
{
    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        if (card.HasPilot())
        {
            Card embarkedPilot = card.GetEmbarkedPilot()!;
            GameState.Disembark(embarkedPilot.ActiveData.Owner, embarkedPilot.InstanceId, false, this);
        }
    }
}