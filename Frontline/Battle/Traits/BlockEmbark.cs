namespace Frontline.Battle.Traits;

public class BlockEmbark : BaseTraitEffect
{
    public override bool CanDeploy(CardStack target, Region region)
    {
        return target.PrimaryCard == null;
    }

    public override bool CanEmbark()
    {
        return false;
    }
}