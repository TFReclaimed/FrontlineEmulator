namespace Frontline.Battle.Traits;

public class InterceptPassive : BaseTraitEffect
{
    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        base.Apply(card, source, active);
        GameState.GetBattleEffects().Add(active);
    }

    public override void Init(Card card, Card source, ActiveTrait active)
    {
        GameState.GetBattleEffects().Add(active);
    }

    public override void Deactivate(ActiveTrait active)
    {
        GameState.GetBattleEffects().Remove(active);
        base.Deactivate(active);
    }

    public override bool IsIntercept(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return false;
        }

        return true;
    }
}