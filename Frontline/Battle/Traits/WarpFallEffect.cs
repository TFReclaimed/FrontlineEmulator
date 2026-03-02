namespace Frontline.Battle.Traits;

public class WarpFallEffect : BaseTraitEffect
{
    public override bool CanDeployOverride(Region region)
    {
        return region == Region.Control;
    }
}