namespace Frontline.Battle.Traits;

public class IgnoreInterceptPassive : BaseTraitEffect
{
    public override bool IgnoreIntercept(ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return false;
        }

        return true;
    }
}