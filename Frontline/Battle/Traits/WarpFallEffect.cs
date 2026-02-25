namespace Frontline.Battle.Traits;

public class WarpFallEffect : BaseTraitEffect
{
    public override bool CanDeployOverride(Region region)
    {
        if (region == Region.Control)
        {
            return true;
        }

        return false;
    }
}