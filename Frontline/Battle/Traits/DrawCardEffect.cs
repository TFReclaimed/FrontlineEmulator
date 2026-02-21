namespace Frontline.Battle.Traits;

public class DrawCardEffect : BaseTraitEffect
{
    public sbyte numberOfCards = 1;

    public bool regularDraw = true;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        Player player = GameState.players[source.activeData.owner];
        sbyte drawCount = numberOfCards;
        if (numberOfCards > 0 && active.dataValue > 0)
        {
            drawCount = (sbyte) active.dataValue;
        }

        if (regularDraw)
        {
            player.DrawFromDeck(source.activeData.owner, drawCount, false);
        }
        else
        {
            drawCount = 1;
            player.supportDeck.DrawCard(player.resources.commandAccum, false);
        }
    }
}