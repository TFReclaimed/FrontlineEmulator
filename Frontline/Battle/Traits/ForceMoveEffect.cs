using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class ForceMoveEffect : BaseTraitEffect
{
    public TargetableArea moveLocation = TargetableArea.AnyAreas;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte owner = source.activeData.owner;
        sbyte owner2 = card.activeData.owner;
        RegionEnum regionEnum = RegionEnum.NumRegions;
        bool titanOnly = card.GetTemplate().Type == CardType.Titan;
        if (moveLocation == TargetableArea.Frontline)
        {
            regionEnum = RegionEnum.Control;
        }
        else if (moveLocation == TargetableArea.FriendlyPerimeter)
        {
            regionEnum = (RegionEnum) (0 + (byte) owner);
        }
        else if (moveLocation == TargetableArea.EnemyPerimeter)
        {
            regionEnum = (RegionEnum) (0 + (byte) GameState.GetOpponentPlayerIndex(owner));
        }

        sbyte b = (sbyte) GameState.board.regions[(uint) regionEnum]
            .GetEmptyCardStackIndex(titanOnly, card.GetTemplate().IsSupportUnit());
        if (b >= 0)
        {
            GameState.Move(owner2, card.instanceId, regionEnum, b, 1, this);
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
                if (list[i].primaryCard.EqualsTo(card))
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