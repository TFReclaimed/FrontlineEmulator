using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class DeployCardEffect : BaseTraitEffect
{
    public TargetableArea deployLocation = TargetableArea.AnyAreas;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte owner = source.activeData.owner;
        sbyte owner2 = card.activeData.owner;
        RegionEnum regionEnum = RegionEnum.NumRegions;
        bool titanOnly = card.GetTemplate().Type == CardType.Titan;
        if (deployLocation == TargetableArea.Frontline)
        {
            regionEnum = RegionEnum.Control;
        }
        else if (deployLocation == TargetableArea.FriendlyPerimeter)
        {
            regionEnum = (RegionEnum) (0 + (byte) owner);
        }

        List<Card> secrets = card.GetSecrets();
        if (secrets != null && secrets.Count > 0)
        {
            for (int i = 0; i < secrets.Count; i++)
            {
                secrets[i].Discard(GameState.players);
            }

            secrets.Clear();
        }

        if (card.HasPilot())
        {
            UnitCard unitCard = (UnitCard) card;
            unitCard.embarkedPilot.Discard(GameState.players);
            unitCard.embarkedPilot = null;
        }

        sbyte b = (sbyte) GameState.board.regions[(uint) regionEnum]
            .GetEmptyCardStackIndex(titanOnly, card.GetTemplate().IsSupportUnit());
        card.ResetCard();
        if (b >= 0)
        {
            GameState.Deploy(owner2, card.instanceId, 0, 0, TargetableArea.CurrentRegion, regionEnum, b, 1, this);
        }
    }
}