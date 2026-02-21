namespace Frontline.Battle.Traits;

public class BlockEmbark : BaseTraitEffect
{
    public override bool CanDeploy(CardStack target, RegionEnum region)
    {
        if (target.primaryCard != null)
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