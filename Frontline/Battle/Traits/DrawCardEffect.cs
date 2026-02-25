namespace Frontline.Battle.Traits;

public class DrawCardEffect : BaseTraitEffect
{
    public sbyte NumberOfCards { get; set; } = 1;

    public bool RegularDraw { get; set; } = true;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var player = GameState.Players[source.ActiveData.Owner];
        var drawCount = NumberOfCards;
        if (NumberOfCards > 0 && active.DataValue > 0)
        {
            drawCount = (sbyte) active.DataValue;
        }

        if (RegularDraw)
        {
            player.DrawFromDeck(source.ActiveData.Owner, drawCount, false);
        }
        else
        {
            drawCount = 1;
            player.SupportDeck.DrawCard(player.Resources.CommandAccum, false);
        }
    }
}