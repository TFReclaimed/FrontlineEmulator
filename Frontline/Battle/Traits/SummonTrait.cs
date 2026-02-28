namespace Frontline.Battle.Traits;

public class SummonTrait : BaseTraitEffect
{
    public sbyte Count { get; set; }

    public override void Activate(Card card, CardStack target, Region region)
    {
        var checkRange = false;
        var onDeploy = true;
        CheckAndApplyTrait(card, card, checkRange, onDeploy);
    }

    public override bool DoesApply(Card card, Card source, bool checkRange, bool onDeploy)
    {
        var owner = source.ActiveData.Owner;
        var targetID = Targets.TargetId;
        var traitActorRegion = GameState.GetTraitActorRegion(owner, source.InstanceId);
        var area = Targets.Area;
        return GameState.CanSummon(owner, targetID, traitActorRegion, area);
    }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var owner = source.ActiveData.Owner;
        var targetID = Targets.TargetId;
        var traitActorRegion = GameState.GetTraitActorRegion(owner, source.InstanceId);
        var area = Targets.Area;
        var b = Count;
        if (Count > 0 && active.DataValue > 0)
        {
            b = (sbyte) active.DataValue;
        }

        if (b > 1)
        {
            while (b > 0)
            {
                if (GameState.CanSummon(owner, targetID, traitActorRegion, area) &&
                    !GameState.Summon(owner, targetID, traitActorRegion, area, this))
                {
                    GameState.Logger.Warning("SummonTrait failed when it should have worked");
                }

                b--;
            }
        }
        else if (!GameState.Summon(owner, targetID, traitActorRegion, area, this))
        {
            GameState.Logger.Warning("SummonTrait failed when it should have worked");
        }
    }
}