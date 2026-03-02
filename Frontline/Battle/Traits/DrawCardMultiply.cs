namespace Frontline.Battle.Traits;

public class DrawCardMultiply : DrawCardEffect
{
    public required TraitTargeting CountInfo { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var player = GameState.Players[source.ActiveData.Owner];
        var drawCount = (sbyte) CountInfo.CalculateCount(GameState, active);
        if (drawCount > 0 && active.DataValue > 0)
        {
            drawCount = (sbyte) (active.DataValue * drawCount);
        }

        player.DrawFromDeck(source.ActiveData.Owner, drawCount, false);
    }
}