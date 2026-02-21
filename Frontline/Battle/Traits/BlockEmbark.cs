namespace Frontline.Battle.Traits;

public class BlockEmbark : BaseTraitEffect
{
    public override bool CanDeploy(CardStack target, RegionEnum region)
    {
        if (target.PrimaryCard != null)
        {
            return false;
        }

        return true;
    }

    public override bool CanEmbark()
    {
        return false;
    }
}