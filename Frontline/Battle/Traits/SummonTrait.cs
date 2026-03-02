namespace Frontline.Battle.Traits;

public class SummonTrait : BaseTraitEffect
{
    public sbyte Count { get; set; }

    public override void Activate(Card card, CardStack? target, Region region)
    {
        var checkRange = false;
        var onDeploy = true;
        CheckAndApplyTrait(card, card, checkRange, onDeploy);
    }

    public override bool DoesApply(Card card, Card source, bool checkRange, bool onDeploy)
    {
        var owner = source.ActiveData.Owner;
        var targetId = Targets.TargetId;
        var traitActorRegion = GameState.GetTraitActorRegion(owner, source.InstanceId);
        var area = Targets.Area;
        return GameState.CanSummon(owner, targetId, traitActorRegion, area);
    }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var owner = source.ActiveData.Owner;
        var targetId = Targets.TargetId;
        var traitActorRegion = GameState.GetTraitActorRegion(owner, source.InstanceId);
        var area = Targets.Area;
        var count = Count;
        if (Count > 0 && active.DataValue > 0)
        {
            count = (sbyte) active.DataValue;
        }

        if (count > 1)
        {
            while (count > 0)
            {
                if (GameState.CanSummon(owner, targetId, traitActorRegion, area) &&
                    !GameState.Summon(owner, targetId, traitActorRegion, area, this))
                {
                    GameState.Logger.Warning("SummonTrait failed when it should have worked");
                }

                count--;
            }
        }
        else if (!GameState.Summon(owner, targetId, traitActorRegion, area, this))
        {
            GameState.Logger.Warning("SummonTrait failed when it should have worked");
        }
    }
}