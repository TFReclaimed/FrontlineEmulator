namespace Frontline.Battle;

public class ActiveUnitCardData : ActiveEntityCardData
{
    public sbyte currentDefense;

    public override void Setup(Card card)
    {
        base.Setup(card);
        UnitCard unitCard = (UnitCard) card;
        currentDefense = unitCard.GetMaxDefense();
    }
}