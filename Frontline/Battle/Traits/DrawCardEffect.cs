namespace Frontline.Battle.Traits;

public class DrawCardEffect : BaseTraitEffect
{
    public sbyte numberOfCards = 1;

    public bool regularDraw = true;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        Player player = GameState.Players[source.ActiveData.Owner];
        sbyte drawCount = numberOfCards;
        if (numberOfCards > 0 && active.DataValue > 0)
        {
            drawCount = (sbyte) active.DataValue;
        }

        if (regularDraw)
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