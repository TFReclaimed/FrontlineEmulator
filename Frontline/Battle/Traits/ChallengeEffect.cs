namespace Frontline.Battle.Traits;

public class ChallengeEffect : BaseTraitEffect
{
    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        CardStack? cardStack = null;
        var list = GameState.FindCardStack(source);
        if (list.Count > 0)
        {
            cardStack = list[0];
        }

        if (cardStack == null)
        {
            return;
        }

        while (card.GetCurrentHealth(false) > 0 && source.GetCurrentHealth(false) > 0)
        {
            source.Attack(cardStack, card);
        }
    }
}