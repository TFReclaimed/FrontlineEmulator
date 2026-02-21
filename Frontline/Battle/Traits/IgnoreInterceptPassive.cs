namespace Frontline.Battle.Traits;

public class IgnoreInterceptPassive : BaseTraitEffect
{
    public override bool IgnoreIntercept(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return false;
        }

        return true;
    }
}