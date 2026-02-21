namespace Frontline.Battle.Traits;

public class DrawCardMultiply : DrawCardEffect
{
    public TraitTargeting countInfo;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        Player player = GameState.Players[source.ActiveData.Owner];
        sbyte b = (sbyte) countInfo.CalculateCount(GameState, active);
        if (b > 0 && active.DataValue > 0)
        {
            b = (sbyte) (active.DataValue * b);
        }

        player.DrawFromDeck(source.ActiveData.Owner, b, false);
    }
}