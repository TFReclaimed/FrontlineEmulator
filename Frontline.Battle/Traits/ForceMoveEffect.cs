using Frontline.Battle.Data.Card;

namespace Frontline.Battle.Traits;

public class ForceMoveEffect : BaseTraitEffect
{
    public TargetableArea MoveLocation { get; set; } = TargetableArea.AnyAreas;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var owner = source.ActiveData.Owner;
        var owner2 = card.ActiveData.Owner;
        var region = Region.NumRegions;
        var titanOnly = card.GetTemplate().Type == CardType.Titan;
        if (MoveLocation == TargetableArea.Frontline)
        {
            region = Region.Control;
        }
        else if (MoveLocation == TargetableArea.FriendlyPerimeter)
        {
            region = (Region) (0 + (byte) owner);
        }
        else if (MoveLocation == TargetableArea.EnemyPerimeter)
        {
            region = (Region) (0 + (byte) GameState.GetOpponentPlayerIndex(owner));
        }

        var index = (sbyte) GameState.Board.Regions[(uint) region]
            .GetEmptyCardStackIndex(titanOnly, card.GetTemplate().IsSupportUnit());
        if (index >= 0)
        {
            GameState.Move(owner2, card.InstanceId, region, index, 1, this);
        }
    }

    public override bool DoesApply(Card card, Card source, bool checkRange, bool onDeploy)
    {
        if (!base.DoesApply(card, source, checkRange, onDeploy))
        {
            return false;
        }

        var result = false;
        var list = GameState.FindCardStack(card);
        foreach (var cardStack in list)
        {
            if (cardStack.PrimaryCard!.EqualsTo(card))
            {
                result = true;
                break;
            }
        }

        return result;

    }
}