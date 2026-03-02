using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class DeployCardEffect : BaseTraitEffect
{
    public TargetableArea DeployLocation { get; set; } = TargetableArea.AnyAreas;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var owner = source.ActiveData.Owner;
        var owner2 = card.ActiveData.Owner;
        var region = Region.NumRegions;
        var titanOnly = card.GetTemplate().Type == CardType.Titan;
        if (DeployLocation == TargetableArea.Frontline)
        {
            region = Region.Control;
        }
        else if (DeployLocation == TargetableArea.FriendlyPerimeter)
        {
            region = (Region) (0 + (byte) owner);
        }

        var secrets = card.GetSecrets();
        if (secrets.Count > 0)
        {
            foreach (var secret in secrets)
            {
                secret.Discard(GameState.Players);
            }

            secrets.Clear();
        }

        if (card.HasPilot())
        {
            var unitCard = (UnitCard) card;
            unitCard.EmbarkedPilot!.Discard(GameState.Players);
            unitCard.EmbarkedPilot = null;
        }

        var index = (sbyte) GameState.Board.Regions[(uint) region]
            .GetEmptyCardStackIndex(titanOnly, card.GetTemplate().IsSupportUnit());
        card.ResetCard();
        if (index >= 0)
        {
            GameState.Deploy(owner2, card.InstanceId, 0, 0, TargetableArea.CurrentRegion,
                region, index, 1, this);
        }
    }
}