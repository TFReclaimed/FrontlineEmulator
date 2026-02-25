using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class ForceMoveEffect : BaseTraitEffect
{
    public TargetableArea MoveLocation { get; set; } = TargetableArea.AnyAreas;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte owner = source.ActiveData.Owner;
        sbyte owner2 = card.ActiveData.Owner;
        Region region = Region.NumRegions;
        bool titanOnly = card.GetTemplate().Type == CardType.Titan;
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

        sbyte b = (sbyte) GameState.Board.Regions[(uint) region]
            .GetEmptyCardStackIndex(titanOnly, card.GetTemplate().IsSupportUnit());
        if (b >= 0)
        {
            GameState.Move(owner2, card.InstanceId, region, b, 1, this);
        }
    }

    public override bool DoesApply(Card card, Card source, bool checkRange, bool onDeploy)
    {
        if (base.DoesApply(card, source, checkRange, onDeploy))
        {
            bool result = false;
            List<CardStack> list = GameState.FindCardStack(card);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].PrimaryCard.EqualsTo(card))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        return false;
    }
}