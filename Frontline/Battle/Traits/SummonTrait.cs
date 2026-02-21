namespace Frontline.Battle.Traits;

public class SummonTrait : BaseTraitEffect
{
    public sbyte count;

    public override void Activate(Card card, CardStack target, RegionEnum region)
    {
        bool checkRange = false;
        bool onDeploy = true;
        CheckAndApplyTrait(card, card, checkRange, onDeploy);
    }

    public override bool DoesApply(Card card, Card source, bool checkRange, bool onDeploy)
    {
        sbyte owner = source.activeData.owner;
        int targetID = targets.targetID;
        RegionEnum traitActorRegion = GameState.GetTraitActorRegion(owner, source.instanceId);
        TargetableArea area = targets.area;
        return GameState.CanSummon(owner, targetID, traitActorRegion, area);
    }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte owner = source.activeData.owner;
        int targetID = targets.targetID;
        RegionEnum traitActorRegion = GameState.GetTraitActorRegion(owner, source.instanceId);
        TargetableArea area = targets.area;
        sbyte b = count;
        if (count > 0 && active.dataValue > 0)
        {
            b = (sbyte) active.dataValue;
        }

        if (b > 1)
        {
            while (b > 0)
            {
                if (GameState.CanSummon(owner, targetID, traitActorRegion, area) &&
                    !GameState.Summon(owner, targetID, traitActorRegion, area, this))
                {
                    Console.WriteLine("SummonTrait failed when it should have worked");
                }

                b--;
            }
        }
        else if (!GameState.Summon(owner, targetID, traitActorRegion, area, this))
        {
            Console.WriteLine("SummonTrait failed when it should have worked");
        }
    }
}