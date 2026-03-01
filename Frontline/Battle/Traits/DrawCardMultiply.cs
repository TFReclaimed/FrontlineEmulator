namespace Frontline.Battle.Traits;

public class DrawCardMultiply : DrawCardEffect
{
    public required TraitTargeting CountInfo { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var player = GameState.Players[source.ActiveData.Owner];
        var b = (sbyte) CountInfo.CalculateCount(GameState, active);
        if (b > 0 && active.DataValue > 0)
        {
            b = (sbyte) (active.DataValue * b);
        }

        player.DrawFromDeck(source.ActiveData.Owner, b, false);
    }
}