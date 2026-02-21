namespace Frontline.Battle;

public class ActiveUnitCardData : ActiveEntityCardData
{
    public sbyte CurrentDefense { get; set; }

    public override void Setup(Card card)
    {
        base.Setup(card);
        UnitCard unitCard = (UnitCard) card;
        CurrentDefense = unitCard.GetMaxDefense();
    }
}