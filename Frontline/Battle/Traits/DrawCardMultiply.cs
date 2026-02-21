namespace Frontline.Battle.Traits;

public class DrawCardMultiply : DrawCardEffect
{
    public TraitTargeting countInfo;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        Player player = GameState.players[source.activeData.owner];
        sbyte b = (sbyte) countInfo.CalculateCount(GameState, active);
        if (b > 0 && active.dataValue > 0)
        {
            b = (sbyte) (active.dataValue * b);
        }

        player.DrawFromDeck(source.activeData.owner, b, false);
    }
}