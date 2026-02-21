namespace Frontline.Battle.Traits;

public class ForceCombatEffect : BaseTraitEffect
{
    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        CardStack cardStack = null;
        List<CardStack> list = GameState.FindCardStack(source);
        if (list.Count > 0)
        {
            cardStack = list[0];
        }

        if (cardStack != null)
        {
            source.Attack(cardStack, card);
        }
    }
}