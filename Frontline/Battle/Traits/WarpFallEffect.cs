namespace Frontline.Battle.Traits;

public class WarpFallEffect : BaseTraitEffect
{
    public override bool CanDeployOverride(RegionEnum region)
    {
        if (region == RegionEnum.Control)
        {
            return true;
        }

        return false;
    }
}