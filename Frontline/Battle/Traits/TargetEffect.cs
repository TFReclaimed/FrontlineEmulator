namespace Frontline.Battle.Traits;

public class TargetEffect : BaseTraitEffect
{
    public bool dropAnywhere;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        GameState.CaptureTemporaryEffect(active);
        base.Apply(card, source, active);
    }

    public override bool TargetTrait()
    {
        return true;
    }
}