namespace Frontline.Battle.Traits;

public class IgnoreInterceptPassive : BaseTraitEffect
{
    public override bool IgnoreIntercept(ActiveTrait active)
    {
        return !Deterable || !active.Detered;
    }
}